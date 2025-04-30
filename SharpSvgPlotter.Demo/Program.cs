using SharpSvgPlotter;
using SharpSvgPlotter.AxisLabeling;
using SharpSvgPlotter.Primitives;

var xData = new List<double> { 1, 2, 3, 4, 5 };
var yData = new List<double> { 2, 3, 5, 7, 11 };
var dataPoints = DataPoint.FromList(xData, yData).ToList();

var plot = new Plot(new PlotOptions
{
    Width = 800,
    Height = 600,
    Title = "Sample Plot",
    BackgroundColor = "#FFFFFF",
    AxisLabelFontSize = 12,
    AxisLabelTickCount = 5,
    AxisLabelFormatString = "G3",
    LabelingAlgorithm = AxisLabelingAlgorithmType.HeckBert,
});
plot.SetXAxis("X Axis");
plot.SetYAxis("Y Axis");

plot.AddLineSeries("Sample Line", dataPoints, new PlotStyle
{
    StrokeColor = "blue",
    StrokeWidth = 2,
});

plot.Save("sample_plot.svg");
Console.WriteLine("Plot saved as sample_plot.svg");