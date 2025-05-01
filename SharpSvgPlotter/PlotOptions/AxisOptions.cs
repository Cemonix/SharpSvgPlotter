using System;

namespace SharpSvgPlotter.PlotOptions;

public class AxisOptions
{
    public double StrokeWidth { get; set; } = 1.0;
    public string Color { get; set; } = "black";
    public double TitleFontSize { get; set; } = 12.0;
    public string TitleColor { get; set; } = "black";
    public string TitleFontFamily { get; set; } = "Arial";

    // --- Ticks ---
    public double TickLength { get; set; } = 5.0;
    public double TickStrokeWidth { get; set; } = 1.0;
    public string TickColor { get; set; } = "black";
    public double TickLabelFontSize { get; set; } = 10.0;
    public string TickLabelColor { get; set; } = "black";
    public string TickLabelFontFamily { get; set; } = "Arial";
    public int TickCountHint { get; set; } = 5; // Number of ticks to show on the axis
    public string TickFormatString { get; set; } = "G3"; // Format string for tick labels

    // --- Axis Label Offsets ---
    // Extra offsets for axis labels. 
    // If set to 0, the label is centered on the axis
    // If value is too large, the label may be cut off
    public double XAxisTitleHorizontalOffset { get; set; } = 0.0; // Offset left of axis line
    public double XAxisTitleVerticalOffset { get; set; } = 0.0; // Offset below axis line
    public double YAxisTitleHorizontalOffset { get; set; } = 0.0; // Offset left of axis line
    public double YAxisTitleVerticalOffset { get; set; } = 0.0; // Offset below axis line
}
