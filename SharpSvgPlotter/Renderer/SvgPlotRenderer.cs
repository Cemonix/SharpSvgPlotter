using System;
using System.Globalization;
using System.Security;
using System.Text;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.Renderer;

public static class SvgPlotRenderer
{
    // TODO: Send option object to set these values
    private const double DefaultAxisStrokeWidth = 1.0;
    private const double DefaultTickLength = 5.0;
    private const double XAxisLabelVerticalOffset = 20.0; // Offset below axis line
    private const double YAxisLabelHorizontalOffset = 10.0; // Offset left of axis line
    private const double AxisTitleFontSize = 12.0;
    private const double TickLabelFontSize = 10.0;
    private const double PlotTitleFontSize = 16.0;

    public static string Render(Plot plot, PlotArea plotArea, ScaleTransform scale)
    {
        // --- 1. Initialization & Setup ---
        if (plot.XAxis == null || plot.YAxis == null)
            throw new InvalidOperationException("Axes must be configured before rendering.");

        var svg = new StringBuilder();
        var culture = CultureInfo.InvariantCulture; // IMPORTANT for decimals in SVG

        // --- 2. SVG Root Element ---
        svg.AppendLine(
            $"<svg width=\"{plot.Width.ToString(culture)}\"" +
            $"height=\"{plot.Height.ToString(culture)}\"" +
            "xmlns=\"http://www.w3.org/2000/svg\">"
        );

        // --- 3. Background ---
        svg.AppendLine(
            $"  <rect x=\"0\" y=\"0\" width=\"{plot.Width.ToString(culture)}\" " +
            $"height=\"{plot.Height.ToString(culture)}\" fill=\"{plot.BackgroundColor}\" />"
        );

        // --- 4. Plot Title ---
        if (!string.IsNullOrEmpty(plot.Title))
        {
            double titleX = plot.Width / 2;
            double titleY = plot.Margins.Top / 2;
            svg.AppendLine(
                $"  <text x=\"{titleX.ToString(culture)}\" y=\"{titleY.ToString(culture)}\" " +
                $"text-anchor=\"middle\" font-size=\"{PlotTitleFontSize.ToString(culture)}\" fill=\"black\">" +
                $"{SecurityElement.Escape(plot.Title)}</text>"
            );
        }

        // --- Define Clip Path (before use) ---
        svg.AppendLine("  <defs>");
        svg.AppendLine($"    <clipPath id=\"plotAreaClip\">");
        svg.AppendLine(
            $"      <rect x=\"{plotArea.X.ToString(culture)}\" y=\"{plotArea.Y.ToString(culture)}\" " +
            $"width=\"{plotArea.Width.ToString(culture)}\" height=\"{plotArea.Height.ToString(culture)}\" />"
        );
        svg.AppendLine($"    </clipPath>");
        svg.AppendLine("  </defs>");

        // --- 5. Render Axes ---
        svg.Append(RenderXAxis(plot.XAxis, plot.YAxis, plotArea, scale, culture));
        svg.Append(RenderYAxis(plot.YAxis, plot.XAxis, plotArea, scale, culture));

        // --- 6. Plot Area Border ---
        svg.AppendLine(
            $"  <rect x=\"{plotArea.X.ToString(culture)}\" y=\"{plotArea.Y.ToString(culture)}\" " +
            $"width=\"{plotArea.Width.ToString(culture)}\" height=\"{plotArea.Height.ToString(culture)}\" " +
            $"fill=\"none\" stroke=\"gray\" stroke-width=\"1\" />"
        );

        // --- 7. Legend Area (Placeholder Group) ---
        // TODO: Example: Top right corner, adjust translate values
        double legendX = plotArea.X + plotArea.Width + 10;
        double legendY = plotArea.Y;
        svg.AppendLine(
            $"  <g id=\"legend\" transform=\"translate({legendX.ToString(culture)}, {legendY.ToString(culture)})\">"
        );
        // Legend items added later
        svg.AppendLine("  </g>");

        // --- 8. Data Series Area (Placeholder Group with Clipping) ---
        svg.AppendLine($"  <g id=\"data-series\" clip-path=\"url(#plotAreaClip)\">");
        // TODO: Render actual series data here in a later step
        // Example call: svg.Append(RenderLineSeries(series, scale, culture));
        svg.AppendLine("  </g>");

        // --- 9. Closing Tag ---
        svg.AppendLine("</svg>");

        return svg.ToString();
    }

    private static string RenderXAxis(
        Axis xAxis, Axis yAxis, PlotArea plotArea, ScaleTransform scale, CultureInfo culture
    ) {
        var sb = new StringBuilder();
        double xPos = plotArea.X;
        double yPos = plotArea.Y + plotArea.Height;

        // Axis Line
        sb.AppendLine(
            $"    <line x1=\"{plotArea.X.ToString(culture)}\" y1=\"{yPos.ToString(culture)}\" " +
            $"x2=\"{(plotArea.X + plotArea.Width).ToString(culture)}\" y2=\"{yPos.ToString(culture)}\" " +
            $"stroke=\"black\" stroke-width=\"{DefaultAxisStrokeWidth.ToString(culture)}\" />"
        );

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
            sb.AppendLine(
                $"    <line x1=\"{pixelX.ToString(culture)}\" y1=\"{yPos.ToString(culture)}\" " +
                $"x2=\"{pixelX.ToString(culture)}\" y2=\"{(yPos + DefaultTickLength).ToString(culture)}\" " +
                $"stroke=\"black\" stroke-width=\"{DefaultAxisStrokeWidth.ToString(culture)}\" />"
            );

            // Tick Label Text
            sb.AppendLine(
                $"    <text x=\"{pixelX.ToString(culture)}\" y=\"{(yPos + XAxisLabelVerticalOffset).ToString(culture)}\" " +
                $"text-anchor=\"middle\" font-size=\"{TickLabelFontSize.ToString(culture)}\" fill=\"black\">{SecurityElement.Escape(label)}</text>"
            );
        }

         // Axis Label
        if (!string.IsNullOrEmpty(xAxis.Label))
        {
            double labelX = xPos + plotArea.Width / 2;
            // Position below tick labels, adjust margin division factor as needed
            double labelY = yPos + plotArea.Height + plotArea.Height * 0.1 + TickLabelFontSize;
            sb.AppendLine(
                $"    <text x=\"{labelX.ToString(culture)}\" y=\"{labelY.ToString(culture)}\" " +
                $"text-anchor=\"middle\" font-size=\"{AxisTitleFontSize.ToString(culture)}\" fill=\"black\">" +
                $"{SecurityElement.Escape(xAxis.Label)}</text>"
            );
        }

        return sb.ToString();
    }

    private static string RenderYAxis(
        Axis yAxis, Axis xAxis, PlotArea plotArea, ScaleTransform scale, CultureInfo culture
    ) {
        var sb = new StringBuilder();
        double xPos = plotArea.X;
        double yPos = plotArea.Y + plotArea.Height;

        // Axis Line
        sb.AppendLine(
            $"    <line x1=\"{xPos.ToString(culture)}\" y1=\"{plotArea.Y.ToString(culture)}\" " +
            $"x2=\"{xPos.ToString(culture)}\" y2=\"{(plotArea.Y + plotArea.Height).ToString(culture)}\" " +
            $"stroke=\"black\" stroke-width=\"{DefaultAxisStrokeWidth.ToString(culture)}\" />"
        );

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
            sb.AppendLine(
                $"    <line x1=\"{(xPos - DefaultTickLength).ToString(culture)}\" y1=\"{pixelY.ToString(culture)}\" " +
                $"x2=\"{xPos.ToString(culture)}\" y2=\"{pixelY.ToString(culture)}\" stroke=\"black\" " +
                $"stroke-width=\"{DefaultAxisStrokeWidth.ToString(culture)}\" />"
            );

            // Tick Label Text
            sb.AppendLine(
                $"    <text x=\"{(xPos - YAxisLabelHorizontalOffset).ToString(culture)}\" y=\"{pixelY.ToString(culture)}\" " +
                $"text-anchor=\"end\" dominant-baseline=\"middle\" font-size=\"{TickLabelFontSize.ToString(culture)}\" " + 
                $"fill=\"black\">{SecurityElement.Escape(label)}</text>"
            );
        }

         // Axis Label
         if (!string.IsNullOrEmpty(yAxis.Label))
         {
            // Position left of tick labels, centered vertically, adjust margin division factor
            double labelX = xPos - YAxisLabelHorizontalOffset - TickLabelFontSize; // Further left
            double labelY = yPos + plotArea.Height / 2;
            string rotateTransform = $"transform=\"rotate(-90 {labelX.ToString(culture)} {labelY.ToString(culture)})\"";
            sb.AppendLine(
                $"    <text x=\"{labelX.ToString(culture)}\" y=\"{labelY.ToString(culture)}\" " +
                $"{rotateTransform} text-anchor=\"middle\" font-size=\"{AxisTitleFontSize.ToString(culture)}\" fill=\"black\">" +
                $"{SecurityElement.Escape(yAxis.Label)}</text>"
            );
         }

        return sb.ToString();
    }
}
