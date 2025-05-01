using System;

namespace SharpSvgPlotter.Styles;

/// <summary>
/// Styling options specifically for line series.
/// Inherits common properties from PlotStyle.
/// </summary>
public class LinePlotStyle(
    string strokeColor = "black",
    double strokeWidth = 1.0,
    string strokeDashArray = "none",
    double strokeOpacity = 1.0,
    string fillColor = "none",
    double fillOpacity = 1.0,
    string fontFamily = "Arial",
    double fontSize = 12.0,
    string textAnchor = "middle"
) : PlotStyle(strokeColor, strokeWidth, fillColor, fillOpacity, strokeOpacity, fontFamily, fontSize, textAnchor)
{
    // --- Line-Specific Properties ---
    public string StrokeDashArray { get; init; } = strokeDashArray;

    public override string ToString()
    {
        return $"{base.ToString()}, Dash: {StrokeDashArray}";
    }
}
