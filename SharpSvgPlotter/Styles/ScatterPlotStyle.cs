using System;

namespace SharpSvgPlotter.Styles;

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

    public ScatterPlotStyle(
        string strokeColor = "black",
        double strokeWidth = 1.0,
        string fillColor = "none",
        double fillOpacity = 1.0,
        double strokeOpacity = 1.0,
        string fontFamily = "Arial",
        double fontSize = 12.0,
        string textAnchor = "middle",
        MarkerType markerShape = MarkerType.Circle,
        double markerSize = 5.0
    ) : base(
        strokeColor: strokeColor,
        strokeWidth: strokeWidth,
        fillColor: fillColor,
        fillOpacity: fillOpacity,
        strokeOpacity: strokeOpacity,
        fontFamily: fontFamily,
        fontSize: fontSize,
        textAnchor: textAnchor
    )
    {
        MarkerShape = markerShape;
        MarkerSize = markerSize;
    }

    public override string ToString()
    {
        return $"Marker: {MarkerShape}, Size: {MarkerSize}";
    }
}