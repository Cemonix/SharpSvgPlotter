using System;
using SharpSvgPlotter.AxisLabeling;
using SharpSvgPlotter.Primitives;

namespace SharpSvgPlotter;

public class PlotOptions
{
    public double Width { get; set; } = 800;
    public double Height { get; set; } = 600;
    public PlotMargins Margins { get; set; } = new (50, 50, 50, 50);
    public string Title { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public int AxisLabelFontSize { get; set; } = 12;
    public int AxisLabelTickCount { get; set; } = 5;
    public string AxisLabelFormatString { get; set; } = "G3";
    public AxisLabelingAlgorithmType? LabelingAlgorithm { get; set; }
}
