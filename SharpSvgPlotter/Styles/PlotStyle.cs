using System;

namespace SharpSvgPlotter.Styles;

/// <summary>
/// Base class for plot styling options.
/// Contains common properties applicable to various plot elements.
/// </summary>
public class PlotStyle(
    string strokeColor = "black",
    double strokeWidth = 1.0,
    string fillColor = "none",
    double fillOpacity = 1.0,
    double strokeOpacity = 1.0,
    string fontFamily = "Arial",
    double fontSize = 12.0,
    string textAnchor = "start"
) {
    // --- Common Style Properties ---
    public string StrokeColor { get; init; } = strokeColor;
    public double StrokeWidth { get; init; } = strokeWidth;
    public string FillColor { get; init; } = fillColor;
    public double FillOpacity { get; init; } = fillOpacity;
    public double StrokeOpacity { get; init; } = strokeOpacity;

    // --- Text Properties ---
    // TODO: Might be less relevant here, consider a separate TextStyle?
    public string FontFamily { get; init; } = fontFamily;
    public double FontSize { get; init; } = fontSize;
    public string TextAnchor { get; init; } = textAnchor;

    public override string ToString()
    {
        return $"Stroke: {StrokeColor}, Width: {StrokeWidth}, Fill: {FillColor}";
    }
}
