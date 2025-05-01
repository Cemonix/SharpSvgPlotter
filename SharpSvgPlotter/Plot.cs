using SharpSvgPlotter.AxisLabeling;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Styles;
using SharpSvgPlotter.Renderer;
using SharpSvgPlotter.Series;
using Options = SharpSvgPlotter.PlotOptions;
using SharpSvgPlotter.Series.HistogramSupport;

namespace SharpSvgPlotter;

public class Plot(Options.PlotOptions options)
{
    private readonly List<ISeries> _series = [];

    public Options.PlotOptions Options { get; init; } = options ?? throw new ArgumentNullException(nameof(options));

    public double Width => Options.Width;
    public double Height => Options.Height;
    public PlotMargins Margins => Options.Margins;
    public string Title => Options.TitleOptions.Title;
    public string BackgroundColor => Options.BackgroundColor;

    public IReadOnlyList<ISeries> Series => _series.AsReadOnly();


    internal Axis? XAxis { get; private set; }
    internal Axis? YAxis { get; private set; }

    /// <summary>
    /// Configures the X-axis.
    /// </summary>
    /// <param name="label">Label text for the axis.</param>
    /// <param name="autoScale">Whether the axis range should be calculated automatically (default true).</param>
    public void SetXAxis(string label, bool autoScale = true) {
        if (Options.LabelingAlgorithm == null)
            throw new InvalidOperationException("Labeling algorithm must be set before configuring axes.");
        
        var axisLabelingOpts = new AxisLabelingOptions() {
            FontSize = Options.XAxisOptions.TickLabelFontSize,
            TickCount = Options.XAxisOptions.TickCountHint,
            FormatString = Options.XAxisOptions.TickFormatString
        };

        XAxis = new Axis(
            label,
            GetLabelingAlgorithm(Options.LabelingAlgorithm),
            axisLabelingOpts,
            autoScale
        );
    }

    /// <summary>
    /// Configures the Y-axis.
    /// </summary>
    /// <param name="label">Label text for the axis.</param>
    /// <param name="autoScale">Whether the axis range should be calculated automatically (default true).</param>
    public void SetYAxis(string label, bool autoScale = true) {
        if (Options.LabelingAlgorithm == null)
            throw new InvalidOperationException("Labeling algorithm must be set before configuring axes.");

        var axisLabelingOpts = new AxisLabelingOptions() {
            FontSize = Options.YAxisOptions.TickLabelFontSize,
            TickCount = Options.YAxisOptions.TickCountHint,
            FormatString = Options.YAxisOptions.TickFormatString
        };

        YAxis = new Axis(
            label,
            GetLabelingAlgorithm(Options.LabelingAlgorithm),
            axisLabelingOpts,
            autoScale
        );
    }

    /// <summary>
    /// Adds a series to the plot.
    /// </summary>
    /// <param name="series">The series object to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when series is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when X or Y axis is not set.</exception>
    public void AddSeries(ISeries series)
    {
        if (series == null)
            throw new ArgumentNullException(nameof(series), "Series cannot be null.");
        if (XAxis == null || YAxis == null)
            throw new InvalidOperationException("Both X and Y axes must be set before adding a series.");
        _series.Add(series);
    }

    /// <summary>
    /// Adds a LineSeries to the plot.
    /// </summary>
    /// <param name="title">Series title (for legend, etc.).</param>
    /// <param name="dataPoints">The list of data points.</param>
    /// <param name="plotStyle">Styling for the series.</param>
    public void AddLineSeries(string title, List<DataPoint> dataPoints, LinePlotStyle plotStyle)
    {
        if (XAxis == null || YAxis == null)
            throw new InvalidOperationException("Both X and Y axes must be set before adding a series.");

        if (dataPoints == null || dataPoints.Count == 0)
            throw new ArgumentException("Data points cannot be null or empty.", nameof(dataPoints));

        var series = new LineSeries(title, dataPoints, plotStyle);
        _series.Add(series);
    }

    /// <summary>
    /// Adds a ScatterSeries to the plot.
    /// </summary>
    /// <param name="title">Series title.</param>
    /// <param name="dataPoints">The list of data points.</param>
    /// <param name="plotStyle">Styling for the series markers.</param>
    public void AddScatterSeries(string title, List<DataPoint> dataPoints, ScatterPlotStyle plotStyle)
    {
        if (XAxis == null || YAxis == null)
            throw new InvalidOperationException("Both X and Y axes must be set before adding a series.");

        if (dataPoints == null || dataPoints.Count == 0)
            throw new ArgumentException("Data points cannot be null or empty.", nameof(dataPoints));

        var series = new ScatterSeries(title, dataPoints, plotStyle);
        _series.Add(series);
    }

    /// <summary>
    /// Adds a HistogramSeries to the plot.
    /// </summary>
    /// <param name="title">Series title.</param>
    /// <param name="data">The list of data points.</param>
    /// <param name="binningMode">Binning mode for the histogram.</param>
    /// <param name="autoRule">Automatic binning rule.</param>
    /// <param name="manualBinCount">Manual bin count (if applicable).</param>
    /// <param name="manualBinWidth">Manual bin width (if applicable).</param>
    /// <param name="plotStyle">Styling for the histogram.</param>
    public void AddHistogramSeries(
        string title,
        List<double> data,
        HistogramStyle plotStyle,
        HistogramBinningMode binningMode = HistogramBinningMode.Automatic,
        AutomaticBinningRule autoRule = AutomaticBinningRule.FreedmanDiaconis,
        int? manualBinCount = null,
        double? manualBinWidth = null
    ) {
        if (XAxis == null || YAxis == null)
            throw new InvalidOperationException("Both X and Y axes must be set before adding a series.");

        if (data == null || data.Count == 0)
            throw new ArgumentException("Data points cannot be null or empty.", nameof(data));

        var series = new HistogramSeries(
            title, data, binningMode, autoRule, manualBinCount, manualBinWidth, plotStyle
        );
        _series.Add(series);
    }

    public void Save(string filePath)
    {
        if (XAxis == null || YAxis == null)
            throw new InvalidOperationException(
                "Both X and Y axes must be configured using SetXAxis/SetYAxis before saving."
            );
         if (_series.Count == 0) {
            Console.Error.WriteLine("Warning: Saving plot with no data series added.");
            return;
         }

        // 1. Prepare Plot Area
        PlotArea plotArea = CalculatePlotArea();

        // 2. Prepare Axes
        PrepareAxes(plotArea);

        // 3. Create Scale Transform
        ScaleTransform scale = CreateScaleTransform(plotArea);

        // 4. Render SVG
        string svgContent = SvgPlotRenderer.Render(this, plotArea, scale);

        // 5. Save File
        File.WriteAllText(filePath, svgContent);
    }

    private static AxisLabelingAlgorithm GetLabelingAlgorithm(AxisLabelingAlgorithmType? algorithmType)
    {
        return algorithmType switch
        {
            AxisLabelingAlgorithmType.HeckBert => new HeckBertAlgorithm(),
            AxisLabelingAlgorithmType.GnuPlot => new GnuPlotAlgorithm(),
            AxisLabelingAlgorithmType.Matplotlib => new MatplotlibAlgorithm(),
            _ => throw new ArgumentException("Invalid labeling algorithm type.", nameof(algorithmType))
        };
    }

    private PlotArea CalculatePlotArea() => new (Width, Height, Margins);

    private void PrepareAxes(PlotArea plotArea)
    {
        if (XAxis == null || YAxis == null) return;

        foreach (var s in _series)
        {
            s.PrepareData();
        }

        if (XAxis.AutoScale)
            XAxis.CalculateRange(_series, AxisType.X);
        if (YAxis.AutoScale)
            YAxis.CalculateRange(_series, AxisType.Y); 
        
        XAxis.CalculateTicks(plotArea.Width);
        YAxis.CalculateTicks(plotArea.Height);
    }

    private ScaleTransform CreateScaleTransform(PlotArea plotArea)
    {
        return new ScaleTransform(XAxis!, YAxis!, plotArea);
    }
}
