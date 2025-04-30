using System;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Primitives.PlotStyles;

namespace SharpSvgPlotter.Series;

public class ScatterSeries: ISeries {
    public string Title { get; init; }
    public PlotStyle PlotStyle { get; init; }
    public IEnumerable<DataPoint> DataPoints { get; init; }

    public ScatterSeries(string title, IEnumerable<DataPoint> dataPoints, ScatterPlotStyle plotStyle)
    {
        Title = title;
        DataPoints = dataPoints ?? [];
        PlotStyle = plotStyle ?? new ScatterPlotStyle();

        if (plotStyle!.GetType() != typeof(ScatterPlotStyle))
            throw new ArgumentException("plotStyle must be of type ScatterPlotStyle.", nameof(plotStyle));
    }

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
