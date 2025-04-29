using SharpSvgPlotter.AxisLabeling;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Renderer;
using SharpSvgPlotter.Series;

namespace SharpSvgPlotter;

public class Plot(double width, double height, PlotMargins plotMargins)
{
    private readonly List<ISeries> _series = [];
    
    public double Width { get; init; } = width;
    public double Height { get; init; } = height;
    public PlotMargins Margins { get; init; } = plotMargins;
    public string Title { get; private set; } = string.Empty;
    public string BackgroundColor { get; private set; } = "#FFFFFF";
    public AxisLabelingAlgorithm? LabelingAlgorithm { get; private set; }
    // TODO: Create new options class that will consists of all axis options and AxisLabelingOptions will be created from it
    public AxisLabelingOptions? LabelingOptions { get; private set; } 
    public Axis? XAxis { get; private set; }
    public Axis? YAxis { get; private set; }
    public IReadOnlyList<ISeries> Series => _series.AsReadOnly();

    public void SetTitle(string title) => Title = title ?? string.Empty;
    
    public void SetBackgroundColor(string color) => BackgroundColor = color ?? "#FFFFFF";

    public void SetAlgorithm(AxisLabelingAlgorithm algorithm) => LabelingAlgorithm = algorithm;
    public void SetOptions(AxisLabelingOptions options) => LabelingOptions = options;

    /// <summary>
    /// Configures the X-axis.
    /// </summary>
    /// <param name="label">Label text for the axis.</param>
    /// <param name="labelingAlgorithm">The algorithm used for tick generation.</param>
    /// <param name="labelingOptions">Options for the labeling algorithm (optional).</param>
    /// <param name="autoScale">Whether the axis range should be calculated automatically (default true).</param>
    public void SetXAxis(string label, bool autoScale = true) {
        if (LabelingAlgorithm == null)
            throw new InvalidOperationException("Labeling algorithm must be set before configuring axes.");

        XAxis = new Axis(label, LabelingAlgorithm, LabelingOptions, autoScale);
    }

    /// <summary>
    /// Configures the Y-axis.
    /// </summary>
    /// <param name="label">Label text for the axis.</param>
    /// <param name="labelingAlgorithm">The algorithm used for tick generation.</param>
    /// <param name="labelingOptions">Options for the labeling algorithm (optional).</param>
    /// <param name="autoScale">Whether the axis range should be calculated automatically (default true).</param>
    public void SetYAxis(string label, bool autoScale = true) {
        if (LabelingAlgorithm == null)
            throw new InvalidOperationException("Labeling algorithm must be set before configuring axes.");

        YAxis = new Axis(label, LabelingAlgorithm, LabelingOptions, autoScale);
    }

    /// <summary>
    /// Adds a LineSeries to the plot.
    /// </summary>
    /// <param name="title">Series title (for legend, etc.).</param>
    /// <param name="dataPoints">The list of data points.</param>
    /// <param name="plotStyle">Styling for the series.</param>
    public void AddLineSeries(string title, List<DataPoint> dataPoints, PlotStyle plotStyle)
    {
        if (XAxis == null || YAxis == null)
            throw new InvalidOperationException("Both X and Y axes must be set before adding a series.");

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

    public void Save(string filePath)
    {
        if (XAxis == null || YAxis == null)
            throw new InvalidOperationException(
                "Both X and Y axes must be configured using SetXAxis/SetYAxis before saving."
            );
         if (_series.Count == 0) {
            Console.WriteLine("Warning: Saving plot with no data series added.");
            return;
         }

        // 1. Prepare Plot Area
        PlotArea plotArea = CalculatePlotArea();

        // 2. Prepare Axes
        PrepareAxes(plotArea);

        // 3. Create Scale Transform
        ScaleTransform scale = CreateScaleTransform(plotArea);

        // 4. Render SVG (Example)
        string svgContent = SvgPlotRenderer.Render(this, plotArea, scale);

        // 5. Save File (Example)
        File.WriteAllText(filePath, svgContent);
        Console.WriteLine($"Plot saved successfully to {filePath}");
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
