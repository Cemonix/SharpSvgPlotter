using SharpSvgPlotter;
using SharpSvgPlotter.Primitives;

var xData = new List<double> { 1, 2, 3, 4, 5 };
var yData = new List<double> { 2, 3, 5, 7, 11 };
var dataPoints = xData.Zip(yData, (x, y) => new DataPoint(x, y)).ToList();

var plot = new Plot(800, 600, new PlotMargins(50, 50, 50, 50));
plot.SetTitle("Sample Plot");
plot.SetXAxis("X Axis");
plot.SetYAxis("Y Axis");

plot.AddLineSeries("Sample Line", dataPoints, new PlotStyle
{
    StrokeColor = "blue",
    StrokeWidth = 2,
});

plot.Save("sample_plot.svg");
Console.WriteLine("Plot saved as sample_plot.svg");