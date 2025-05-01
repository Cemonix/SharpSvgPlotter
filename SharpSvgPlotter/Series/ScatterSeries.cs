using System;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Styles;

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

    public void PrepareData() { }

    public (double Min, double Max) GetAxisBounds(AxisType axisType)
    {
        var points = DataPoints.ToList();
        if (points.Count == 0)
            return (double.NaN, double.NaN);

        return axisType switch
        {
            AxisType.X => (points.Min(dp => dp.X), points.Max(dp => dp.X)),
            AxisType.Y => (points.Min(dp => dp.Y), points.Max(dp => dp.Y)),
            _ => (double.NaN, double.NaN)
        };
    }
}
