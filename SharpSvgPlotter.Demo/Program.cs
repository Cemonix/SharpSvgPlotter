using SharpSvgPlotter;
using SharpSvgPlotter.AxisLabeling;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Primitives.PlotStyles;

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

    var plot = new Plot(new PlotOptions
    {
        Width = 800,
        Height = 600,
        Title = "Iris Dataset Scatter Plot",
        BackgroundColor = "#FFFFFF",
        AxisLabelFontSize = 12,
        AxisLabelTickCount = 5,
        AxisLabelFormatString = "G3",
        LabelingAlgorithm = AxisLabelingAlgorithmType.HeckBert,
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

IrisScatterPlot();