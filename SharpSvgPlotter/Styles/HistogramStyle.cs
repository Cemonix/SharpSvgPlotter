using System;

namespace SharpSvgPlotter.Styles;

public class HistogramStyle(
    string borderColor = "black",
    double borderWidth = 0.5
) : PlotStyle(
    fillColor: "#1f77b4",
    fillOpacity: 0.75,
    strokeColor: borderColor,
    strokeWidth: borderWidth,
    fontFamily: "Arial",
    fontSize: 12.0,
    textAnchor: "middle",
    strokeOpacity: 1.0
) {
    public string BorderColor { get; set; } = borderColor;
    public double BorderWidth { get; set; } = borderWidth;
}
