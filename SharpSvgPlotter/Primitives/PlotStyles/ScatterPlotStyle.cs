using System;

namespace SharpSvgPlotter.Primitives.PlotStyles;

/// <summary>
/// Defines marker shapes for scatter plots.
/// </summary>
public enum MarkerType
{
    Circle,
    Square,
}

public class ScatterPlotStyle : PlotStyle
{
    // --- Scatter-Specific Marker Properties ---
    public MarkerType MarkerShape { get; init; }
    public double MarkerSize { get; init; }
    public string MarkerFill { get; init; }
    public string MarkerStroke { get; init; }
    public double MarkerStrokeWidth { get; init; }

    public ScatterPlotStyle(
        MarkerType markerShape = MarkerType.Circle,
        double markerSize = 5.0,
        string? markerFill = null,
        string? markerStroke = null,
        double markerStrokeWidth = 1.0,
        double opacity = 1.0,
        string strokeColor = "black",
        double strokeWidth = 1.0,
        string fillColor = "none",
        double fillOpacity = 1.0,
        double strokeOpacity = 1.0,
        string fontFamily = "Arial",
        double fontSize = 12.0,
        string textAnchor = "middle"
    ) : base(strokeColor, strokeWidth, fillColor, fillOpacity, strokeOpacity, fontFamily, fontSize, textAnchor)
    {
        MarkerShape = markerShape;
        MarkerSize = markerSize;
        MarkerFill = markerFill ?? StrokeColor;
        MarkerStroke = markerStroke ?? "none";
        MarkerStrokeWidth = markerStrokeWidth;

        // TODO: Consider if opacity should be applied to fill and stroke separately
        FillOpacity = opacity;
        StrokeOpacity = opacity;
    }

    public override string ToString()
    {
        return $"Marker: {MarkerShape}, Size: {MarkerSize}, Fill: {MarkerFill ?? "default"}, Stroke: {MarkerStroke ?? "default"}";
    }
}
