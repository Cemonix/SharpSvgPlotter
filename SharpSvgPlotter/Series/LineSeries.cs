using System;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Styles;

namespace SharpSvgPlotter.Series;

public class LineSeries : ISeries
{
    public string Title { get; init; }
    public PlotStyle PlotStyle { get; init; }
    public IEnumerable<DataPoint> DataPoints { get; init; }

    public LineSeries(string title, IEnumerable<DataPoint> dataPoints, LinePlotStyle plotStyle)
    {
        Title = title;
        DataPoints = dataPoints ?? [];
        PlotStyle = plotStyle ?? new LinePlotStyle();

        if (plotStyle!.GetType() != typeof(LinePlotStyle))
            throw new ArgumentException("plotStyle must be of type LinePlotStyle.", nameof(plotStyle));
    }

    // No preparation needed for simple line series
    public void PrepareData() { }

    // Calculate bounds from internal DataPoints
    public (double Min, double Max) GetAxisBounds(AxisType axisType)
    {
        var points = DataPoints.ToList();
        if (points.Count == 0)
            return (double.NaN, double.NaN); // No contribution if no data

        return axisType switch
        {
            AxisType.X => (points.Min(dp => dp.X), points.Max(dp => dp.X)),
            AxisType.Y => (points.Min(dp => dp.Y), points.Max(dp => dp.Y)),
            _ => (double.NaN, double.NaN) // Should not happen
        };
    }
}