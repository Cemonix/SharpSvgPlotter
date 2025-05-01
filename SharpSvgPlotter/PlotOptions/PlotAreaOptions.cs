using System;

namespace SharpSvgPlotter.PlotOptions;

public class PlotAreaOptions
{
    public string BorderColor { get; set; } = "gray";
    public double BorderWidth { get; set; } = 1.0;
    public string FillColor { get; set; } = "none"; // Typically transparent
}
