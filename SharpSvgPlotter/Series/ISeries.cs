using System;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Primitives.PlotStyles;

namespace SharpSvgPlotter.Series;

public interface ISeries
{
    string Title { get; init; }
    PlotStyle PlotStyle { get; init; }
    IEnumerable<DataPoint> DataPoints { get; init; }
    (double minX, double maxX, double minY, double maxY) GetDataRange();
}
