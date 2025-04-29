using System;

namespace SharpSvgPlotter.AxisLabeling;

/// <summary>
/// Options to configure axis labeling algorithms.
/// </summary>
public class AxisLabelingOptions
{
    /// <summary>
    /// Target density of labels (e.g., labels per inch or per 100 pixels).
    /// Used by density-aware algorithms like Wilkinson Extended.
    /// Set to null if not applicable or using default.
    /// </summary>
    public double? TargetDensity { get; set; }

    /// <summary>
    /// Target font size for legibility calculations (if implemented).
    /// </summary>
    public double? TargetFontSize { get; set; } // In points or pixels

    /// <summary>
    /// A hint for the desired number of ticks (used by simpler algorithms).
    /// </summary>
    public int? DesiredTickCount { get; set; } = 5; // Default hint

    public string? FormatString { get; set; } = "G3"; // Default format string for labels
}
