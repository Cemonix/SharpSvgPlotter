using System;

namespace SharpSvgPlotter.Primitives.PlotStyles;

/// <summary>
/// Base class for plot styling options.
/// Contains common properties applicable to various plot elements.
/// </summary>
public class PlotStyle
{
    // --- Common Style Properties ---
    public string StrokeColor { get; init; }
    public double StrokeWidth { get; init; }
    public string FillColor { get; init; }
    public double FillOpacity { get; init; }
    public double StrokeOpacity { get; init; }

    // --- Text Properties ---
    // TODO: Might be less relevant here, consider a separate TextStyle?
    public string FontFamily { get; init; }
    public double FontSize { get; init; }
    public string TextAnchor { get; init; }

    public PlotStyle(
        string strokeColor = "black",
        double strokeWidth = 1.0,
        string fillColor = "none",
        double fillOpacity = 1.0,
        double strokeOpacity = 1.0,
        string fontFamily = "Arial",
        double fontSize = 12.0,
        string textAnchor = "start"
    )
    {
        StrokeColor = strokeColor;
        StrokeWidth = strokeWidth;
        FillColor = fillColor;
        FillOpacity = fillOpacity;
        StrokeOpacity = strokeOpacity;
        FontFamily = fontFamily;
        FontSize = fontSize;
        TextAnchor = textAnchor;
    }

    public override string ToString()
    {
        return $"Stroke: {StrokeColor}, Width: {StrokeWidth}, Fill: {FillColor}";
    }
}
