using System;

namespace SharpSvgPlotter.Series.HistogramSupport;

public enum HistogramBinningMode
{
    /// <summary>
    /// Automatically determine bin width/count using a chosen rule.
    /// </summary>
    Automatic,
    /// <summary>
    /// Use a specified number of bins.
    /// </summary>
    ManualCount,
    /// <summary>
    /// Use a specified width for bins.
    /// </summary>
    ManualWidth
}