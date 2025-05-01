using SharpSvgPlotter;
using SharpSvgPlotter.AxisLabeling;
using SharpSvgPlotter.PlotOptions;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Primitives.PlotStyles;

static void SineWavePlot()
{   
    // --- 1. Create Plot and Configure Axes ---
    var titleOptions = new TitleOptions
    {
        Title = "Sine Wave Plot",
        FontSize = 16,
        Color = "black",
        FontFamily = "Arial"
    };

    var legendOptions = new LegendOptions
    {
        ShowLegend = true,
    };

    var plot = new Plot(new PlotOptions
    {
        Width = 800,
        Height = 600,
        BackgroundColor = "#FFFFFF",
        LabelingAlgorithm = AxisLabelingAlgorithmType.Matplotlib,
        TitleOptions = titleOptions,
        LegendOptions = legendOptions,
    });
    plot.SetXAxis("X");
    plot.SetYAxis("Y");

    // --- 2. Generate Data Points for the Sine Wave ---
    List<DataPoint> sineData = [];

    // --- Parameters to Change ---
    double cycles = 4;                 // Number of cycles to display
    double frequencyMultiplier = 1;    // Frequency multiplier for the sine wave
    int pointsPerCycle = 100;          // Minimum points needed for one smooth cycle visually
    // --------------------------

    double startX = 0;
    double endX = cycles * 2 * Math.PI;
    int numberOfPoints = (int)(cycles * pointsPerCycle);

    if (numberOfPoints < 2) numberOfPoints = 2;

    double stepX = (endX - startX) / (numberOfPoints - 1);

    Console.WriteLine($"Plotting {cycles} cycles from {startX:F2} to {endX:F2} ({numberOfPoints} points)...");

    for (int i = 0; i < numberOfPoints; i++)
    {
        double currentX = startX + i * stepX;
        double currentY = Math.Sin(frequencyMultiplier * currentX);
        sineData.Add(new DataPoint(currentX, currentY));
    }

    // --- 3. Define the Style ---
    var lineStyle = new LinePlotStyle(
        strokeColor: "green",
        strokeWidth: 2.0
    );

    // --- 4. Add the Data as a LineSeries ---
    plot.AddLineSeries("Sine Wave", sineData, lineStyle);

    // --- 5. Save the Plot ---
    plot.Save("sine_wave_plot.svg");

    Console.WriteLine("Sine wave plot generated!");
}

static void IrisScatterPlot()
{
    string filePath = "Data/Iris.csv";

    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File not found: {filePath}");
        return;
    }

    var data = File.ReadAllLines(filePath)
        .Skip(1)
        .Select(line => line.Split(','))
        .Select(parts => new
        {
            SepalLength = double.Parse(parts[0]),
            SepalWidth = double.Parse(parts[1]),
            PetalLength = double.Parse(parts[2]),
            PetalWidth = double.Parse(parts[3]),
            Species = parts[4]
        })
        .ToList();

    var dataPoints = data.Select(d => new DataPoint(d.SepalLength, d.SepalWidth)).ToList();

    var titleOptions = new TitleOptions
    {
        Title = "Iris Dataset Scatter Plot",
        FontSize = 16,
        Color = "black",
        FontFamily = "Arial"
    };
    var legendOptions = new LegendOptions
    {
        ShowLegend = true,
    };

    var plot = new Plot(new PlotOptions
    {
        Width = 800,
        Height = 600,
        BackgroundColor = "#FFFFFF",
        LabelingAlgorithm = AxisLabelingAlgorithmType.HeckBert,
        TitleOptions = titleOptions,
        LegendOptions = legendOptions,
    });
    plot.SetXAxis("Sepal Length");
    plot.SetYAxis("Sepal Width");

    plot.AddScatterSeries("Iris Data", dataPoints, new ScatterPlotStyle
    {
        FillColor = "blue",
        FillOpacity = 0.5,
        StrokeColor = "black",
        StrokeWidth = 1,
        StrokeOpacity = 0.8,
    });

    plot.Save("iris_scatter_plot.svg");
    Console.WriteLine("Iris scatter plot saved as iris_scatter_plot.svg");
}

SineWavePlot();
IrisScatterPlot();