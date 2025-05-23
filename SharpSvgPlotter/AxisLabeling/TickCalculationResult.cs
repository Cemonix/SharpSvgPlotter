using System;

namespace SharpSvgPlotter.AxisLabeling;

/// <summary>
/// Represents the calculated ticks and labels for an axis.
/// </summary>
internal class TickCalculationResult
{
    /// <summary>
    /// The calculated positions of the tick marks along the axis data range.
    /// </summary>
    internal List<double> TickPositions { get; init; } = [];

    /// <summary>
    /// The formatted labels corresponding to each tick position.
    /// </summary>
    internal List<string> TickLabels { get; init; } = [];

    /// <summary>
    /// The minimum value of the axis range potentially adjusted by the algorithm.
    /// </summary>
    internal double ActualMin { get; init; }

    /// <summary>
    /// The maximum value of the axis range potentially adjusted by the algorithm.
    /// </summary>
    internal double ActualMax { get; init; }

    internal TickCalculationResult(List<double> positions, List<string> labels, double min, double max)
    {
        TickPositions = positions ?? [];
        TickLabels = labels ?? [];

        if (TickPositions.Count != TickLabels.Count)
            throw new ArgumentException("TickPositions and TickLabels must have the same count.");
        
        ActualMin = min;
        ActualMax = max;
    }
}