using System;

namespace SharpSvgPlotter.PlotOptions;

public enum LegendLocation
{
    TopRight,
    TopLeft,
    BottomRight,
    BottomLeft,
}

public class LegendOptions
{
    public bool ShowLegend { get; set; } = false;
    public LegendLocation Location { get; set; } = LegendLocation.TopRight;

    // --- Positioning & Padding ---
    public double LocationPaddingX { get; set; } = 10.0; // Padding from plot area edge (horizontal)
    public double LocationPaddingY { get; set; } = 10.0; // Padding from plot area edge (vertical)
    public double InternalPaddingX { get; set; } = 5.0; // Padding inside the legend box (left/right)
    public double InternalPaddingY { get; set; } = 5.0; // Padding inside the legend box (top/bottom)

    // --- Styling ---
    public double FontSize { get; set; } = 10.0;
    public string FontColor { get; set; } = "black";
    public string FontFamily { get; set; } = "Arial";
    public double ItemHeight { get; set; } = 15.0;
    public double SymbolXOffset { get; set; } = 5.0; // X pos of symbol within content area
    public double SymbolTextGap { get; set; } = 5.0; // Gap between symbol and text start
    public double SymbolSize { get; set; } = 10.0;

    // --- Legend Box ---
    public string BackgroundColor { get; set; } = "white";
    public double BackgroundOpacity { get; set; } = 0.85;
    public string BorderColor { get; set; } = "gray";
    public double BorderWidth { get; set; } = 0.5;
}
