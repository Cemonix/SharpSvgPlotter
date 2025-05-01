using System;

namespace SharpSvgPlotter.PlotOptions;

public class TitleOptions
{
    public string Title { get; set; } = string.Empty;
    public double FontSize { get; set; } = 16.0;
    public string Color { get; set; } = "black";
    public string FontFamily { get; set; } = "Arial";
}
