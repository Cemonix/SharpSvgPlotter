using System;

namespace SharpSvgPlotter.AxisLabeling;

/// <summary>
/// Options to configure axis labeling algorithms.
/// </summary>
internal class AxisLabelingOptions(
    double? density = null,
    double? fontSize = null,
    int tickCount = 5,
    string formatString = "G3"
) {
    /// <summary>
    /// Density of labels (e.g., labels per inch or per 100 pixels).
    /// Used by density-aware algorithms like Wilkinson Extended.
    /// </summary>
    internal double? Density { get; init; } = density;

    /// <summary>
    /// Font size for legibility calculations.
    /// </summary>
    internal double? FontSize { get; init; } = fontSize;

    /// <summary>
    /// A hint for the number of ticks (used by simpler algorithms).
    /// </summary>
    internal int? TickCount { get; init; } = tickCount;

    /// <summary>
    /// Format string for tick labels.
    /// </summary>
    internal string? FormatString { get; init; } = formatString;
}
