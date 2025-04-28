using System;
using SharpSvgPlotter.Primitives;

namespace SharpSvgPlotter.Series;

public class LineSeries(string title, List<DataPoint> dataPoints, PlotStyle plotStyle) : ISeries
{
    public string Title { get; init; } = title;
    public PlotStyle PlotStyle { get; init; } = plotStyle;
    public IEnumerable<DataPoint> DataPoints { get; init; } = dataPoints ?? [];

    public (double minX, double maxX, double minY, double maxY) GetDataRange()
    {   
        var points = DataPoints.ToList();
        if (points.Count == 0)
            return (0, 0, 0, 0);

        double minX = points.Min(dp => dp.X);
        double maxX = points.Max(dp => dp.X);
        double minY = points.Min(dp => dp.Y);
        double maxY = points.Max(dp => dp.Y);

        return (minX, maxX, minY, maxY);
    }
}