using System;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Series.HistogramSupport;
using SharpSvgPlotter.Styles;

namespace SharpSvgPlotter.Series;

public class HistogramSeries  : ISeries
{
    public string Title { get; init; }
    public PlotStyle PlotStyle { get; init; }
    public IEnumerable<double> Data { get; init; }

    public HistogramBinningMode BinningMode { get; init; }
    public int? ManualBinCount { get; init; }
    public double? ManualBinWidth { get; init; }
    public AutomaticBinningRule AutoBinningRule { get; init; }

    internal List<HistogramBin> CalculatedBins { get; private set; } = [];
    private double _calculatedXMin = double.NaN;
    private double _calculatedXMax = double.NaN;
    private double _calculatedYMax = double.NaN; // YMin is always 0

    public HistogramSeries(
        string title,
        IEnumerable<double> data,
        HistogramBinningMode binningMode = HistogramBinningMode.Automatic,
        AutomaticBinningRule autoRule = AutomaticBinningRule.FreedmanDiaconis,
        int? manualBinCount = null,
        double? manualBinWidth = null,
        HistogramStyle? style = null
    ) {
        Title = title ?? string.Empty;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        BinningMode = binningMode;
        AutoBinningRule = autoRule;
        ManualBinCount = manualBinCount;
        ManualBinWidth = manualBinWidth;
        PlotStyle = style ?? new HistogramStyle();

        if (BinningMode == HistogramBinningMode.ManualCount && !ManualBinCount.HasValue)
            throw new ArgumentException("ManualBinCount must be provided for ManualCount mode.");
        if (BinningMode == HistogramBinningMode.ManualWidth && !ManualBinWidth.HasValue)
            throw new ArgumentException("ManualBinWidth must be provided for ManualWidth mode.");
    }

    /// <summary>
    /// Calculates bins and stores bounds needed for axis scaling.
    /// </summary>
    public void PrepareData()
    {
        CalculatedBins = HistogramBinner.GenerateBins(
            Data, BinningMode, ManualBinCount, ManualBinWidth, AutoBinningRule
        );

        if (CalculatedBins.Count > 0)
        {
            _calculatedXMin = CalculatedBins[0].LowerBound;
            _calculatedXMax = CalculatedBins[^1].UpperBound;
            _calculatedYMax = CalculatedBins.Max(b => b.Count);
        }
        else
        {
            _calculatedXMin = double.NaN;
            _calculatedXMax = double.NaN;
            _calculatedYMax = double.NaN;
        }
    }

    /// <summary>
    /// Returns the calculated axis bounds after PrepareData has been called.
    /// </summary>
    public (double Min, double Max) GetAxisBounds(AxisType axisType)
    {
        return axisType switch
        {
            AxisType.X => (_calculatedXMin, _calculatedXMax),
            AxisType.Y => (0, _calculatedYMax), // Y axis for histogram starts at 0
            _ => (double.NaN, double.NaN)
        };
    }
}
