using System;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Styles;

namespace SharpSvgPlotter.Series;

public interface ISeries
{
    /// <summary>
    /// Title used for the series and the legend.
    /// </summary>
    string Title { get; init; }

    /// <summary>
    /// Base styling information
    /// </summary>
    PlotStyle PlotStyle { get; init; } // Use the base PlotStyle type

    /// <summary>
    /// Performs any necessary pre-calculations before axis scaling.
    /// (e.g., Histogram binning).
    /// </summary>
    void PrepareData();

    /// <summary>
    /// Gets the minimum and maximum data values this series contributes to an axis.
    /// This is called *after* PrepareData.
    /// </summary>
    /// <param name="axisType">Which axis (X or Y) bounds are required.</param>
    /// <returns>
    /// A tuple (min, max) for the specified axis. 
    /// Returns (NaN, NaN) if the series doesn't contribute to the axis or has no data.
    /// </returns>
    (double Min, double Max) GetAxisBounds(AxisType axisType);
}
