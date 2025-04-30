using SharpSvgPlotter.AxisLabeling;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Primitives.PlotStyles;
using SharpSvgPlotter.Renderer;
using SharpSvgPlotter.Series;

namespace SharpSvgPlotter;

public class Plot(PlotOptions options)
{
    private readonly List<ISeries> _series = [];

    public double Width { get; init; } = options.Width;
    public double Height { get; init; } = options.Height;
    public PlotMargins Margins { get; init; } = options.Margins;
    public string Title { get; init; } = options.Title;
    public string BackgroundColor { get; init; } = options.BackgroundColor;
    public int AxisLabelFontSize { get; init; } = options.AxisLabelFontSize;
    public int AxisLabelTickCount { get; init; } = options.AxisLabelTickCount;
    public string AxisLabelFormatString { get; init; } = options.AxisLabelFormatString;
    public AxisLabelingAlgorithmType? LabelingAlgorithm { get; init; } = options.LabelingAlgorithm;
    public IReadOnlyList<ISeries> Series => _series.AsReadOnly();
    internal Axis? XAxis { get; private set; }
    internal Axis? YAxis { get; private set; }

    /// <summary>
    /// Configures the X-axis.
    /// </summary>
    /// <param name="label">Label text for the axis.</param>
    /// <param name="autoScale">Whether the axis range should be calculated automatically (default true).</param>
    public void SetXAxis(string label, bool autoScale = true) {
        if (LabelingAlgorithm == null)
            throw new InvalidOperationException("Labeling algorithm must be set before configuring axes.");
        
        XAxis = new Axis(
            label,
            GetLabelingAlgorithm(LabelingAlgorithm),
            new AxisLabelingOptions() {
                FontSize = AxisLabelFontSize,
                TickCount = AxisLabelTickCount,
                FormatString = AxisLabelFormatString
            },
            autoScale
        );
    }

    /// <summary>
    /// Configures the Y-axis.
    /// </summary>
    /// <param name="label">Label text for the axis.</param>
    /// <param name="autoScale">Whether the axis range should be calculated automatically (default true).</param>
    public void SetYAxis(string label, bool autoScale = true) {
        if (LabelingAlgorithm == null)
            throw new InvalidOperationException("Labeling algorithm must be set before configuring axes.");

        YAxis = new Axis(
            label,
            GetLabelingAlgorithm(LabelingAlgorithm),
            new AxisLabelingOptions() {
                FontSize = AxisLabelFontSize,
                TickCount = AxisLabelTickCount,
                FormatString = AxisLabelFormatString
            },
            autoScale
        );
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
    /// Adds an existing LineSeries object to the plot.
    /// </summary>
    /// <param name="series">The series object to add.</param>
    public void AddLineSeries(LineSeries series)
    {
        if (series == null)
            throw new ArgumentNullException(nameof(series), "Series cannot be null.");
        if (XAxis == null || YAxis == null)
            throw new InvalidOperationException("Both X and Y axes must be set before adding a series.");
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

        var series = new ScatterSeries(title, dataPoints, plotStyle);
        _series.Add(series);
    }

    public void AddScatterSeries(ScatterSeries series)
    {
        if (series == null)
            throw new ArgumentNullException(nameof(series), "Series cannot be null.");
        if (XAxis == null || YAxis == null)
            throw new InvalidOperationException("Both X and Y axes must be set before adding a series.");
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
        Console.WriteLine($"Plot saved successfully to {filePath}");
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
        if (XAxis!.AutoScale)
            XAxis.CalculateRange(_series, AxisType.X);
        if (YAxis!.AutoScale)
            YAxis.CalculateRange(_series, AxisType.Y);

        XAxis.CalculateTicks(plotArea.Width);
        YAxis.CalculateTicks(plotArea.Height);
    }

    private ScaleTransform CreateScaleTransform(PlotArea plotArea)
    {
        return new ScaleTransform(XAxis!, YAxis!, plotArea);
    }
}
