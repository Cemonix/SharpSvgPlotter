using System;

namespace SharpSvgPlotter.Primitives;

public readonly struct PlotStyle(
    string strokeColor = "black",
    double strokeWidth = 1.0,
    string strokeDashArray = "none",
    string fill = "none",
    string fillOpacity = "1.0",
    string fontFamily = "Arial",
    double fontSize = 12.0,
    string textAnchor = "start"
) {
    public string StrokeColor { get; init; } = strokeColor;
    public double StrokeWidth { get; init; } = strokeWidth;
    public string StrokeDashArray { get; init; } = strokeDashArray;
    public string Fill { get; init; } = fill;
    public string FillOpacity { get; init; } = fillOpacity;
    public string FontFamily { get; init; } = fontFamily;
    public double FontSize { get; init; } = fontSize;
    public string TextAnchor { get; init; } = textAnchor;

    public override readonly string ToString()
    {
        return $"Color: {StrokeColor}, StrokeWidth: {StrokeWidth}, StrokeDashArray: {StrokeDashArray}, " +
            $"Fill: {Fill}, FontFamily: {FontFamily}, FontSize: {FontSize}, TextAnchor: {TextAnchor}";
    }
}
