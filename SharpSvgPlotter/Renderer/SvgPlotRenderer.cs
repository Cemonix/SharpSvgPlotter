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
using SharpSvgPlotter.PlotOptions;

namespace SharpSvgPlotter.Renderer;

internal static class SvgPlotRenderer
{
    private static readonly XNamespace _ns = XNamespace.Get("http://www.w3.org/2000/svg");

    /// <summary>
    /// Renders the plot as an SVG string.
    /// </summary>
    /// <param name="plot">The plot to render.</param>
    /// <param name="plotArea">The area where the plot is drawn.</param>
    /// <param name="scale">The scale transform to apply to the data points.</param>
    /// <returns>An SVG string representing the plot.</returns>
    /// <exception cref="InvalidOperationException">Thrown if axes are not configured.</exception>
    internal static string Render(Plot plot, PlotArea plotArea, ScaleTransform scale)
    {
        // --- Initialization & Setup ---
        if (plot.XAxis == null || plot.YAxis == null)
            throw new InvalidOperationException("Axes must be configured before rendering.");

        var svg = new StringBuilder();
        var options = plot.Options;
        var culture = options.Culture;

        // --- SVG Root Element ---
        var svgRoot = new XElement(_ns + "svg",
            new XAttribute("width", plot.Width.ToString(culture)),
            new XAttribute("height", plot.Height.ToString(culture))
        );

        // --- Background ---
        svgRoot.Add(new XElement(_ns + "rect",
            new XAttribute("x", 0),
            new XAttribute("y", 0),
            new XAttribute("width", plot.Width.ToString(culture)),
            new XAttribute("height", plot.Height.ToString(culture)),
            new XAttribute("fill", plot.BackgroundColor ?? "white")
        ));

        // --- Plot Title ---
        if (!string.IsNullOrEmpty(options.TitleOptions.Title))
        {
            double titleX = plot.Width / 2;
            double titleY = plot.Margins.Top / 2;
            svgRoot.Add(new XElement(_ns + "text",
                new XAttribute("x", titleX.ToString(culture)),
                new XAttribute("y", titleY.ToString(culture)),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("font-family", options.TitleOptions.FontFamily),
                new XAttribute("font-size", options.TitleOptions.FontSize.ToString(culture)),
                new XAttribute("fill", options.TitleOptions.Color),
                options.TitleOptions.Title
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

        // --- Render Axes ---
        svgRoot.Add(RenderXAxis(plot.XAxis, plot.YAxis, plotArea, scale, options.XAxisOptions, culture));
        svgRoot.Add(RenderYAxis(plot.YAxis, plot.XAxis, plotArea, scale, options.XAxisOptions, culture));

        // --- Plot Area Border ---
        svgRoot.Add(new XElement(_ns + "rect",
            new XAttribute("x", plotArea.X.ToString(culture)),
            new XAttribute("y", plotArea.Y.ToString(culture)),
            new XAttribute("width", plotArea.Width.ToString(culture)),
            new XAttribute("height", plotArea.Height.ToString(culture)),
            new XAttribute("fill", options.PlotAreaOptions.FillColor),
            new XAttribute("stroke", options.PlotAreaOptions.BorderColor),
            new XAttribute("stroke-width", options.PlotAreaOptions.BorderWidth.ToString(culture))
        ));

        // --- Legend Area ---
        if (options.LegendOptions.ShowLegend && plot.Series.Any())
        {
            // Calculate Legend Content Size
            (double contentWidth, double contentHeight) = CalculateLegendContentSize(
                plot.Series, options.LegendOptions, culture
            );

            // Calculate Full Legend Box Size (Content + Internal Padding)
            double legendBoxWidth = contentWidth + 2 * options.LegendOptions.InternalPaddingX;
            double legendBoxHeight = contentHeight + 2 * options.LegendOptions.InternalPaddingY;

            // Calculate Legend Top-Left Position (based on Location and Padding)
            double legendX = CalculateLegendX(options.LegendOptions, plotArea, legendBoxWidth);
            double legendY = CalculateLegendY(options.LegendOptions, plotArea, legendBoxHeight);

            // Create Legend Group with Transform
            var legendGroup = new XElement(_ns + "g",
                new XAttribute("id", "legend"),
                new XAttribute("transform", $"translate({legendX.ToString(culture)}, {legendY.ToString(culture)})")
            );

            // Add Background Rectangle (Inside the group, at 0,0 relative to group)
            legendGroup.Add(new XElement(_ns + "rect",
                new XAttribute("x", 0),
                new XAttribute("y", 0),
                new XAttribute("width", legendBoxWidth.ToString(culture)),
                new XAttribute("height", legendBoxHeight.ToString(culture)),
                new XAttribute("fill", options.LegendOptions.BackgroundColor),
                new XAttribute("fill-opacity", options.LegendOptions.BackgroundOpacity.ToString(culture)),
                new XAttribute("stroke", options.LegendOptions.BorderColor),
                new XAttribute("stroke-width", options.LegendOptions.BorderWidth.ToString(culture))
            ));

            // Create Content Group (offset by internal padding)
            var contentGroup = new XElement(_ns + "g",
                new XAttribute("transform",
                $"translate({options.LegendOptions.InternalPaddingX.ToString(culture)}, " +
                $"{options.LegendOptions.InternalPaddingY.ToString(culture)})")
            );

            // Render Legend Items into the Content Group
            RenderLegendItems(contentGroup, plot.Series, options.LegendOptions, culture);

            // Add content group to main legend group
            legendGroup.Add(contentGroup);

            // Add the complete legend group to the SVG Root (AFTER data)
            svgRoot.Add(legendGroup);
        }

        // --- Data Series Area ---
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
                    dataSeriesGroup.Add(seriesPath);
            }
            else if (series is ScatterSeries scatterSeries)
            {
                List<XElement> markers = RenderScatterSeries(scatterSeries, scale, culture);
                dataSeriesGroup.Add(markers);
            }
        }

        // --- Return SVG String ---
        return svgRoot.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>
    /// Renders the X-axis of the plot.
    /// </summary>
    /// <param name="xAxis">The X-axis to render.</param>
    /// <param name="yAxis">The Y-axis to render.</param>
    /// <param name="plotArea">The plot area where the axes are drawn.</param>
    /// <param name="scale">The scale transform to apply to the data points.</param>
    /// <param name="axisOptions">The options for the axis styling.</param>
    /// <param name="culture">The culture info for formatting numbers.</param>
    /// <returns>A list of XElement representing the X-axis elements.</returns>
    private static List<XElement> RenderXAxis(
        Axis xAxis, Axis yAxis, PlotArea plotArea, ScaleTransform scale, AxisOptions axisOptions, CultureInfo culture
    ) {
        double yPos = plotArea.Y + plotArea.Height;
        List<XElement> elements = [];

        // Axis Line
        elements.Add(new XElement(_ns + "line",
            new XAttribute("x1", plotArea.X.ToString(culture)),
            new XAttribute("y1", yPos.ToString(culture)),
            new XAttribute("x2", (plotArea.X + plotArea.Width).ToString(culture)),
            new XAttribute("y2", yPos.ToString(culture)),
            new XAttribute("stroke", axisOptions.Color),
            new XAttribute("stroke-width", axisOptions.StrokeWidth.ToString(culture))
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
                new XAttribute("y2", (yPos + axisOptions.TickLength).ToString(culture)),
                new XAttribute("stroke", "black"),
                new XAttribute("stroke-width", axisOptions.TickStrokeWidth.ToString(culture))
            ));

            // Tick Label Text - Position based on TickLength and font size implicitly
            elements.Add(new XElement(_ns + "text",
                new XAttribute("x", pixelX.ToString(culture)),
                new XAttribute("y", (yPos + axisOptions.TickLength + axisOptions.TickLabelFontSize * 0.8).ToString(culture)),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("font-family", axisOptions.TickLabelFontFamily),
                new XAttribute("font-size", axisOptions.TickLabelFontSize.ToString(culture)),
                new XAttribute("fill", axisOptions.TickLabelColor),
                label
            ));
        }

        // Axis Title
        if (!string.IsNullOrEmpty(xAxis.Label))
        {
            double labelX = plotArea.X + plotArea.Width / 2;
            double labelY = (
                plotArea.Y + 
                plotArea.Height + 
                (plotArea.Margins.Bottom * 0.75) + // Center below the axis
                axisOptions.XAxisTitleHorizontalOffset + // Additional horizontal offset
                axisOptions.XAxisTitleVerticalOffset // Additional vertical offset
            );
            elements.Add(new XElement(_ns + "text",
                new XAttribute("x", labelX.ToString(culture)),
                new XAttribute("y", labelY.ToString(culture)),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("font-family", axisOptions.TitleFontFamily),
                new XAttribute("font-size", axisOptions.TitleFontSize.ToString(culture)),
                new XAttribute("fill", axisOptions.TitleColor),
                xAxis.Label
            ));
        }

        return elements;
    }

    /// <summary>
    /// Renders the Y-axis of the plot.
    /// </summary>
    /// <param name="yAxis">The Y-axis to render.</param>
    /// <param name="xAxis">The X-axis to render.</param>
    /// <param name="plotArea">The plot area where the axes are drawn.</param>
    /// <param name="scale">The scale transform to apply to the data points.</param>
    /// <param name="axisOptions">The options for the axis styling.</param>
    /// <param name="culture">The culture info for formatting numbers.</param>
    /// <returns>A list of XElement representing the Y-axis elements.</returns>
    private static List<XElement> RenderYAxis(
        Axis yAxis, Axis xAxis, PlotArea plotArea, ScaleTransform scale, AxisOptions axisOptions, CultureInfo culture
    ) {
        double xPos = plotArea.X;
        List<XElement> elements = [];

        // Axis Line
        elements.Add(new XElement(_ns + "line",
            new XAttribute("x1", xPos.ToString(culture)),
            new XAttribute("y1", plotArea.Y.ToString(culture)),
            new XAttribute("x2", xPos.ToString(culture)),
            new XAttribute("y2", (plotArea.Y + plotArea.Height).ToString(culture)),
            new XAttribute("stroke", axisOptions.Color),
            new XAttribute("stroke-width", axisOptions.StrokeWidth.ToString(culture))
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
                new XAttribute("x1", (xPos - axisOptions.TickLength).ToString(culture)),
                new XAttribute("y1", pixelY.ToString(culture)),
                new XAttribute("x2", xPos.ToString(culture)),
                new XAttribute("y2", pixelY.ToString(culture)),
                new XAttribute("stroke", axisOptions.TickColor),
                new XAttribute("stroke-width", axisOptions.TickStrokeWidth.ToString(culture))
            ));

            // Tick Label Text - Position based on TickLength and font size
            elements.Add(new XElement(_ns + "text",
                new XAttribute("x", (xPos - axisOptions.TickLength - axisOptions.TickLabelFontSize * 0.5).ToString(culture)),
                new XAttribute("y", pixelY.ToString(culture)),
                new XAttribute("text-anchor", "end"), // Align text right, ending before the tick mark
                new XAttribute("dominant-baseline", "middle"),
                new XAttribute("font-family", axisOptions.TickLabelFontFamily),
                new XAttribute("font-size", axisOptions.TickLabelFontSize.ToString(culture)),
                new XAttribute("fill", axisOptions.TickLabelColor),
                label
            ));
        }

        // Axis Title - Use axisOptions
        if (!string.IsNullOrEmpty(yAxis.Label))
        {
            // Use the dedicated offset from axisOptions
            double labelX = (
                plotArea.X - 
                (plotArea.Margins.Left / 2) - // Centered in the left margin
                axisOptions.YAxisTitleHorizontalOffset - // Additional horizontal offset
                axisOptions.YAxisTitleVerticalOffset // Additional vertical offset
            );
            double labelY = plotArea.Y + plotArea.Height / 2;
            elements.Add(new XElement(_ns + "text",
                new XAttribute("transform", $"translate({labelX.ToString(culture)},{labelY.ToString(culture)}) rotate(-90)"),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("font-family", axisOptions.TitleFontFamily),
                new XAttribute("font-size", axisOptions.TitleFontSize.ToString(culture)),
                new XAttribute("fill", axisOptions.TitleColor),
                yAxis.Label
            ));
        }

        return elements;
    }

    /// <summary>
    /// Calculates the size of the legend content based on the series list and legend options.
    /// </summary>
    /// <param name="seriesList">The list of series to be included in the legend.</param>
    /// <param name="legendOptions">The options for the legend styling.</param>
    /// <param name="culture">The culture info for formatting numbers.</param>
    /// <returns>A tuple containing the estimated width and height of the legend content.</returns>
    // NOTE: Measuring text width accurately is hard. This is an ESTIMATE.
    private static (double Width, double Height) CalculateLegendContentSize(
        IReadOnlyList<ISeries> seriesList, LegendOptions legendOptions, CultureInfo culture)
    {
        if (!seriesList.Any()) return (0, 0);

        double maxWidth = 0;
        double currentY = 0;
        double symbolWidth = legendOptions.SymbolSize * 1.5; // Allow space for line symbol length
        double textX = legendOptions.SymbolXOffset + symbolWidth + legendOptions.SymbolTextGap;

        foreach (var series in seriesList)
        {
            // TODO: Estimate text width (very rough estimate)
            // A more robust way would involve a graphics library or JS, or assuming monospace font
            double estimatedTextWidth = series.Title.Length * legendOptions.FontSize * 0.6; // Adjust multiplier

            double itemWidth = textX + estimatedTextWidth;
            maxWidth = Math.Max(maxWidth, itemWidth);
            currentY += legendOptions.ItemHeight;
        }

        double totalHeight = seriesList.Count * legendOptions.ItemHeight;

        return (maxWidth, totalHeight);
    }

    /// <summary>
    /// Calculates the X position for the legend based on its location and padding.
    /// </summary>
    /// <param name="legendOptions">The legend options containing location and padding.</param>
    /// <param name="plotArea">The plot area where the legend is drawn.</param>
    /// <param name="legendBoxWidth">The width of the legend box.</param>
    /// <returns>The calculated X position for the legend.</returns>
    private static double CalculateLegendX(LegendOptions legendOptions, PlotArea plotArea, double legendBoxWidth)
    {
        return legendOptions.Location switch
        {
            LegendLocation.TopLeft or LegendLocation.BottomLeft =>
                plotArea.X + legendOptions.LocationPaddingX,
            LegendLocation.TopRight or LegendLocation.BottomRight =>
                plotArea.X + plotArea.Width - legendOptions.LocationPaddingX - legendBoxWidth,
            _ => plotArea.X + legendOptions.LocationPaddingX // Default to TopLeft
        };
    }

    /// <summary>
    /// Calculates the Y position for the legend based on its location and padding.
    /// </summary>
    /// <param name="legendOptions">The legend options containing location and padding.</param>
    /// <param name="plotArea">The plot area where the legend is drawn.</param>
    /// <param name="legendBoxHeight">The height of the legend box.</param>
    /// <returns>The calculated Y position for the legend.</returns>
    private static double CalculateLegendY(LegendOptions legendOptions, PlotArea plotArea, double legendBoxHeight)
    {
         return legendOptions.Location switch
         {
            LegendLocation.TopLeft or LegendLocation.TopRight =>
                plotArea.Y + legendOptions.LocationPaddingY,
            LegendLocation.BottomLeft or LegendLocation.BottomRight =>
                plotArea.Y + plotArea.Height - legendOptions.LocationPaddingY - legendBoxHeight,
             _ => plotArea.Y + legendOptions.LocationPaddingY // Default to TopLeft
         };
    }

    /// <summary>
    /// Renders the legend items for the plot.
    /// </summary>
    /// <param name="legendGroup">The SVG group element for the legend.</param>
    /// <param name="seriesList">The list of series to render in the legend.</param>
    /// <param name="legendOptions">The options for the legend styling.</param>
    /// <param name="culture">The culture info for formatting numbers.</param>
    private static void RenderLegendItems(
        XElement legendGroup, IReadOnlyList<ISeries> seriesList, LegendOptions legendOptions, CultureInfo culture
    ) {
        double currentY = 0; // Start Y relative to legend group's origin
        double itemHeight = legendOptions.ItemHeight;
        double symbolX = legendOptions.SymbolXOffset;
        double symbolSize = legendOptions.SymbolSize;
        double symbolEndX = symbolX + (symbolSize * 1.5);
        double textX = symbolEndX + legendOptions.SymbolTextGap;

        foreach (var series in seriesList)
        {
            XElement? symbol = null;
            // Y position for vertical centering within the item's allocated height
            double symbolCenterY = currentY + itemHeight / 2;

            // --- Render Symbol ---
            if (series.PlotStyle is LinePlotStyle lineStyle)
            {
                symbol = new XElement(_ns + "line",
                    new XAttribute("x1", symbolX.ToString(culture)),
                    new XAttribute("y1", symbolCenterY.ToString(culture)),
                    new XAttribute("x2", (symbolX + symbolSize * 1.5).ToString(culture)),
                    new XAttribute("y2", symbolCenterY.ToString(culture)),
                    new XAttribute("stroke", lineStyle.StrokeColor),
                    new XAttribute("stroke-width", lineStyle.StrokeWidth.ToString(culture)),
                    (
                        !string.IsNullOrEmpty(lineStyle.StrokeDashArray) && 
                        !lineStyle.StrokeDashArray.Equals("none", StringComparison.CurrentCultureIgnoreCase)
                    ) ? new XAttribute("stroke-dasharray", lineStyle.StrokeDashArray) : null
                );
            }
            else if (series.PlotStyle is ScatterPlotStyle scatterStyle)
            {
                string shapeName = scatterStyle.MarkerShape.ToString().ToLower();
                symbol = new XElement(_ns + shapeName);
                symbol.Add(new XAttribute("fill", scatterStyle.MarkerFill));
                if (scatterStyle.MarkerStroke != "none") {
                    symbol.Add(new XAttribute("stroke", scatterStyle.MarkerStroke));
                    symbol.Add(new XAttribute("stroke-width", scatterStyle.MarkerStrokeWidth.ToString(culture)));
                }
                symbol.Add(new XAttribute("opacity", scatterStyle.FillOpacity.ToString(culture)));

                if (scatterStyle.MarkerShape == MarkerType.Circle) {
                    symbol.Add(new XAttribute("cx", (symbolX + symbolSize / 2).ToString(culture)));
                    symbol.Add(new XAttribute("cy", symbolCenterY.ToString(culture)));
                    symbol.Add(new XAttribute("r", (symbolSize / 2).ToString(culture)));
                } else if (scatterStyle.MarkerShape == MarkerType.Square) {
                    symbol.Add(new XAttribute("x", symbolX.ToString(culture)));
                    symbol.Add(new XAttribute("y", (symbolCenterY - symbolSize / 2).ToString(culture)));
                    symbol.Add(new XAttribute("width", symbolSize.ToString(culture)));
                    symbol.Add(new XAttribute("height", symbolSize.ToString(culture)));
                }
            }

            if (symbol != null) legendGroup.Add(symbol);

            // --- Render Text ---
            var text = new XElement(_ns + "text",
                new XAttribute("x", textX.ToString(culture)),
                new XAttribute("y", symbolCenterY.ToString(culture)), // Align vertically with symbol center
                new XAttribute("font-family", legendOptions.FontFamily),
                new XAttribute("font-size", legendOptions.FontSize.ToString(culture)),
                new XAttribute("fill", legendOptions.FontColor),
                new XAttribute("dominant-baseline", "middle"),
                new XAttribute("text-anchor", "start"),
                series.Title
            );
            legendGroup.Add(text);

            currentY += itemHeight; // Move down for the next item
        }
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
            (
                !string.IsNullOrEmpty(style.StrokeDashArray) &&
                !style.StrokeDashArray.Equals("none", StringComparison.CurrentCultureIgnoreCase)
            ) ? new XAttribute("stroke-dasharray", style.StrokeDashArray) : null,
            new XAttribute("fill", "none"),
            new XAttribute("stroke-opacity", style.StrokeOpacity.ToString(culture))
        );

        return pathElement;
    }

    /// <summary>
    /// Renders a scatter series as SVG elements.
    /// </summary>
    /// <param name="series">The scatter series to render.</param>
    /// <param name="scale">The scale transform to apply to the data points.</param>
    /// <param name="culture">The culture info for formatting numbers.</param>
    /// <returns>A list of XElement representing the SVG markers for the scatter series.</returns>
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
            string shapeName = markerShape.ToString().ToLower();
            var marker = new XElement(_ns + shapeName);

            // Common attributes from style
            marker.Add(new XAttribute("fill", style.MarkerFill));
            if (style.MarkerStroke != "none") {
                marker.Add(new XAttribute("stroke", style.MarkerStroke));
                marker.Add(new XAttribute("stroke-width", style.MarkerStrokeWidth.ToString(culture)));
            }
            marker.Add(new XAttribute("opacity", style.FillOpacity.ToString(culture)));

            // Shape-specific attributes from style & transformed point
            if (markerShape == MarkerType.Circle) {
                 marker.Add(new XAttribute("cx", transformedPoint.X.ToString(culture)));
                 marker.Add(new XAttribute("cy", transformedPoint.Y.ToString(culture)));
                 marker.Add(new XAttribute("r", style.MarkerSize.ToString(culture)));
            } else if (markerShape == MarkerType.Square) {
                double size = style.MarkerSize;
                marker.Add(new XAttribute("x", (transformedPoint.X - size / 2).ToString(culture)));
                marker.Add(new XAttribute("y", (transformedPoint.Y - size / 2).ToString(culture)));
                marker.Add(new XAttribute("width", size.ToString(culture)));
                marker.Add(new XAttribute("height", size.ToString(culture)));
            }

            markerElements.Add(marker);
        }

        return markerElements;
    }
}
