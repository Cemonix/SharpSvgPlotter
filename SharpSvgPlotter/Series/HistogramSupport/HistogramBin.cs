using System;

namespace SharpSvgPlotter.Series.HistogramSupport;

/// <summary>
/// Represents a single bin in a histogram.
/// </summary>
public readonly struct HistogramBin(double lowerBound, double upperBound, int count)
{
    /// <summary>
    /// The inclusive lower bound of the bin.
    /// </summary>
    public double LowerBound { get; } = lowerBound;
    /// <summary>
    /// The exclusive upper bound of the bin (except possibly for the last bin).
    /// </summary>
    public double UpperBound { get; } = upperBound;
    /// <summary>
    /// The number of data points falling within this bin.
    /// </summary>
    public int Count { get; } = count;

    /// <summary>
    /// The width of the bin (UpperBound - LowerBound).
    /// </summary>
    public double Width => UpperBound - LowerBound;

    public override string ToString() => $"[{LowerBound:G3} - {UpperBound:G3}): {Count}";
}
