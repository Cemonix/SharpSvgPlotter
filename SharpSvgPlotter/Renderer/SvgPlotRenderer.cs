using System;
using System.Xml.Linq;
using System.Globalization;
using System.Security;
using System.Text;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Utils;
using SharpSvgPlotter.Series;
using SharpSvgPlotter.Primitives.PlotStyles;

namespace SharpSvgPlotter.Renderer;

internal static class SvgPlotRenderer
{
    // TODO: Send option object to set these values
    private const double DefaultAxisStrokeWidth = 1.0;
    private const double DefaultTickLength = 5.0;
    private const double XAxisLabelVerticalOffset = 20.0; // Offset below axis line
    private const double YAxisLabelHorizontalOffset = 10.0; // Offset left of axis line
    private const double AxisTitleFontSize = 12.0;
    private const double TickLabelFontSize = 10.0;
    private const double PlotTitleFontSize = 16.0;
    private static readonly XNamespace _ns = XNamespace.Get("http://www.w3.org/2000/svg");

    internal static string Render(Plot plot, PlotArea plotArea, ScaleTransform scale)
    {
        // --- 1. Initialization & Setup ---
        if (plot.XAxis == null || plot.YAxis == null)
            throw new InvalidOperationException("Axes must be configured before rendering.");

        var svg = new StringBuilder();
        var culture = CultureInfo.InvariantCulture; // IMPORTANT for decimals in SVG

        // --- 2. SVG Root Element ---
        var svgRoot = new XElement(_ns + "svg",
            new XAttribute("width", plot.Width.ToString(culture)),
            new XAttribute("height", plot.Height.ToString(culture))
        );

        // --- 3. Background ---
        svgRoot.Add(new XElement(_ns + "rect",
            new XAttribute("x", 0),
            new XAttribute("y", 0),
            new XAttribute("width", plot.Width.ToString(culture)),
            new XAttribute("height", plot.Height.ToString(culture)),
            new XAttribute("fill", plot.BackgroundColor ?? "white")
        ));

        // --- 4. Plot Title ---
        if (!string.IsNullOrEmpty(plot.Title))
        {
            double titleX = plot.Width / 2;
            double titleY = plot.Margins.Top / 2;
            svgRoot.Add(new XElement(_ns + "text",
                new XAttribute("x", titleX.ToString(culture)),
                new XAttribute("y", titleY.ToString(culture)),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("font-size", PlotTitleFontSize.ToString(culture)),
                new XAttribute("fill", "black"),
                plot.Title
            ));
        }

        // --- Define Clip Path (before use) ---
        svgRoot.Add(new XElement(
            _ns + "defs",
            new XElement(
                _ns + "clipPath",
                new XAttribute("id", "plotAreaClip"),
                new XElement(
                    _ns + "rect",
                    new XAttribute("x", plotArea.X.ToString(culture)),
                    new XAttribute("y", plotArea.Y.ToString(culture)),
                    new XAttribute("width", plotArea.Width.ToString(culture)),
                    new XAttribute("height", plotArea.Height.ToString(culture))
                )
            )
        ));

        // --- 5. Render Axes ---
        svgRoot.Add(RenderXAxis(plot.XAxis, plot.YAxis, plotArea, scale, culture));
        svgRoot.Add(RenderYAxis(plot.YAxis, plot.XAxis, plotArea, scale, culture));

        // --- 6. Plot Area Border ---
        svgRoot.Add(new XElement(
            _ns + "rect",
            new XAttribute("x", plotArea.X.ToString(culture)),
            new XAttribute("y", plotArea.Y.ToString(culture)),
            new XAttribute("width", plotArea.Width.ToString(culture)),
            new XAttribute("height", plotArea.Height.ToString(culture)),
            new XAttribute("fill", "none"),
            new XAttribute("stroke", "gray"),
            new XAttribute("stroke-width", "1")
        ));

        // --- 7. Legend Area ---
        // TODO: Example: Top right corner, adjust translate values
        double legendX = plotArea.X + plotArea.Width + 10; // Adjust position
        double legendY = plotArea.Y;
        svgRoot.Add(new XElement(_ns + "g",
            new XAttribute("id", "legend"),
            new XAttribute("transform", $"translate({legendX.ToString(culture)}, {legendY.ToString(culture)})")
            // Add legend items here later by adding children to this XElement
        ));

        // --- 8. Data Series Area ---
        var dataSeriesGroup = new XElement(_ns + "g",
            new XAttribute("id", "data-series"),
            new XAttribute("clip-path", "url(#plotAreaClip)")
        );
        svgRoot.Add(dataSeriesGroup);

        // --- Render Series Data ---
        foreach (var series in plot.Series)
        {
            if (series is LineSeries lineSeries)
            {
                XElement? seriesPath = RenderLineSeries(lineSeries, scale, culture);
                if (seriesPath != null)
                {
                    dataSeriesGroup.Add(seriesPath);
                }
            }
            else if (series is ScatterSeries scatterSeries)
            {
                List<XElement> markers = RenderScatterSeries(scatterSeries, scale, culture);
                foreach (var marker in markers)
                {
                    dataSeriesGroup.Add(marker);
                }
            }
        }

        // --- 9. Return SVG String ---
        return svgRoot.ToString(SaveOptions.DisableFormatting);
    }

    private static List<XElement> RenderXAxis(
        Axis xAxis, Axis yAxis, PlotArea plotArea, ScaleTransform scale, CultureInfo culture
    ) {
        double yPos = plotArea.Y + plotArea.Height;
        List<XElement> elements = [];

        // Axis Line
        elements.Add(new XElement(_ns + "line",
            new XAttribute("x1", plotArea.X.ToString(culture)),
            new XAttribute("y1", yPos.ToString(culture)),
            new XAttribute("x2", (plotArea.X + plotArea.Width).ToString(culture)),
            new XAttribute("y2", yPos.ToString(culture)),
            new XAttribute("stroke", "black"),
            new XAttribute("stroke-width", DefaultAxisStrokeWidth.ToString(culture))
        ));

        // Ticks and Labels
        for (int i = 0; i < xAxis.TickPositions.Count; i++)
        {
            double dataX = xAxis.TickPositions[i];
            string label = xAxis.TickLabels[i];

            DataPoint transformedPoint = scale.Transform(new DataPoint(dataX, yAxis.Min));
            double pixelX = transformedPoint.X;

            // Prevent drawing ticks/labels outside the plot area bounds slightly
            if (pixelX < plotArea.X - Constants.Epsilon || pixelX > plotArea.X + plotArea.Width + Constants.Epsilon)
                continue;


            // Tick Mark
            elements.Add(new XElement(_ns + "line",
                new XAttribute("x1", pixelX.ToString(culture)),
                new XAttribute("y1", yPos.ToString(culture)),
                new XAttribute("x2", pixelX.ToString(culture)),
                new XAttribute("y2", (yPos + DefaultTickLength).ToString(culture)),
                new XAttribute("stroke", "black"),
                new XAttribute("stroke-width", DefaultAxisStrokeWidth.ToString(culture))
            ));

            // Tick Label Text
            elements.Add(new XElement(_ns + "text",
                new XAttribute("x", pixelX.ToString(culture)),
                new XAttribute("y", (yPos + XAxisLabelVerticalOffset).ToString(culture)),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("font-size", TickLabelFontSize.ToString(culture)),
                new XAttribute("fill", "black"),
                label
            ));
        }

         // Axis Label
        if (!string.IsNullOrEmpty(xAxis.Label))
        {
            double labelX = plotArea.X + plotArea.Width / 2;
            double labelY = plotArea.Y + plotArea.Height + (plotArea.Margins.Bottom * 0.75);
            elements.Add(new XElement(_ns + "text",
                new XAttribute("x", labelX.ToString(culture)),
                new XAttribute("y", labelY.ToString(culture)),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("font-size", AxisTitleFontSize.ToString(culture)),
                new XAttribute("fill", "black"),
                xAxis.Label
            ));
        }

        return elements;
    }

    private static List<XElement> RenderYAxis(
        Axis yAxis, Axis xAxis, PlotArea plotArea, ScaleTransform scale, CultureInfo culture
    ) {
        double xPos = plotArea.X;
        List<XElement> elements = [];

        // Axis Line
        elements.Add(new XElement(_ns + "line",
            new XAttribute("x1", xPos.ToString(culture)),
            new XAttribute("y1", plotArea.Y.ToString(culture)),
            new XAttribute("x2", xPos.ToString(culture)),
            new XAttribute("y2", (plotArea.Y + plotArea.Height).ToString(culture)),
            new XAttribute("stroke", "black"),
            new XAttribute("stroke-width", DefaultAxisStrokeWidth.ToString(culture))
        ));

        // Ticks and Labels
        for (int i = 0; i < yAxis.TickPositions.Count; i++)
        {
            double dataY = yAxis.TickPositions[i];
            string label = yAxis.TickLabels[i];

            DataPoint transformedPoint = scale.Transform(new DataPoint(xAxis.Min, dataY));
            double pixelY = transformedPoint.Y;

            // Prevent drawing ticks/labels outside the plot area bounds slightly
            if (pixelY < plotArea.Y - Constants.Epsilon || pixelY > plotArea.Y + plotArea.Height + Constants.Epsilon)
                continue;

            // Tick Mark
            elements.Add(new XElement(_ns + "line",
                new XAttribute("x1", (xPos - DefaultTickLength).ToString(culture)),
                new XAttribute("y1", pixelY.ToString(culture)),
                new XAttribute("x2", xPos.ToString(culture)),
                new XAttribute("y2", pixelY.ToString(culture)),
                new XAttribute("stroke", "black"),
                new XAttribute("stroke-width", DefaultAxisStrokeWidth.ToString(culture))
            ));

            // Tick Label Text
            elements.Add(new XElement(_ns + "text",
                new XAttribute("x", (xPos - YAxisLabelHorizontalOffset).ToString(culture)),
                new XAttribute("y", pixelY.ToString(culture)),
                new XAttribute("text-anchor", "end"),
                new XAttribute("dominant-baseline", "middle"),
                new XAttribute("font-size", TickLabelFontSize.ToString(culture)),
                new XAttribute("fill", "black"),
                label
            ));
        }

         // Axis Label
         if (!string.IsNullOrEmpty(yAxis.Label))
         {
            // Position left of tick labels, centered vertically, adjust margin division factor
            double labelX = plotArea.X - (plotArea.Margins.Left / 2);
            double labelY = plotArea.Y + plotArea.Height / 2;
            elements.Add(new XElement(_ns + "text",
                new XAttribute("transform", $"translate({labelX.ToString(culture)},{labelY.ToString(culture)}) rotate(-90)"),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("font-size", AxisTitleFontSize.ToString(culture)),
                new XAttribute("fill", "black"),
                yAxis.Label
            ));
         }

        return elements;
    }

    /// <summary>
    /// Renders a line series as an SVG path element.
    /// </summary>
    /// <param name="series">The line series to render.</param>
    /// <param name="scale">The scale transform to apply to the data points.</param>
    /// <param name="culture">The culture info for formatting numbers.</param>
    /// <returns>An XElement representing the SVG path for the line series.</returns>
    private static XElement? RenderLineSeries(LineSeries series, ScaleTransform scale, CultureInfo culture)
    {
        if (series.DataPoints == null || !series.DataPoints.Any())
        {
        return null;
        }

        var pathData = new StringBuilder();
        bool firstPoint = true;

        foreach (DataPoint dataPoint in series.DataPoints)
        {
            DataPoint transformedPoint = scale.Transform(dataPoint);
            if (firstPoint)
            {
                pathData.Append($"M {transformedPoint.X.ToString(culture)} {transformedPoint.Y.ToString(culture)}");
                firstPoint = false;
            }
            else
            {
                pathData.Append($" L {transformedPoint.X.ToString(culture)} {transformedPoint.Y.ToString(culture)}");
            }
        }

        // Get style from the series
        LinePlotStyle style = series.PlotStyle as LinePlotStyle ?? new LinePlotStyle();

        // Create the XElement for the path
        var pathElement = new XElement(_ns + "path",
            new XAttribute("d", pathData.ToString()),
            new XAttribute("stroke", style.StrokeColor),
            new XAttribute("stroke-width", style.StrokeWidth.ToString(culture)),
            (!string.IsNullOrEmpty(style.StrokeDashArray)) ? new XAttribute("stroke-dasharray", style.StrokeDashArray) : null,
            new XAttribute("fill", "none")
        );

        return pathElement;
    }

    private static List<XElement> RenderScatterSeries(ScatterSeries series, ScaleTransform scale, CultureInfo culture)
    {
        List<XElement> markerElements = [];
        if (series.DataPoints == null) return markerElements;

        // Get style from the series
        ScatterPlotStyle style = series.PlotStyle as ScatterPlotStyle ?? new ScatterPlotStyle();

        MarkerType markerShape = style.MarkerShape;

        foreach (DataPoint dataPoint in series.DataPoints)
        {
            DataPoint transformedPoint = scale.Transform(dataPoint);

            var marker = new XElement(_ns + markerShape.ToString().ToLower(),
                new XAttribute("cx", transformedPoint.X.ToString(culture)),
                new XAttribute("cy", transformedPoint.Y.ToString(culture)),
                new XAttribute("r", style.MarkerSize.ToString(culture)),
                new XAttribute("fill", style.FillColor),
                (style.StrokeColor != "none") ? new XAttribute("stroke", style.StrokeColor) : null,
                (style.StrokeWidth > 0 && style.StrokeColor != "none") ? new XAttribute(
                    "stroke-width", style.StrokeWidth.ToString(culture)
                ) : null,
                new XAttribute("opacity", style.FillOpacity.ToString(culture))
            );
            markerElements.Add(marker);
        }

        return markerElements;
    }
}
