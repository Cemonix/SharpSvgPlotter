using System;

namespace SharpSvgPlotter.Primitives;

public readonly struct DataPoint(double x, double y)
{
    public double X { get; } = x;
    public double Y { get; } = y;

    public override readonly string ToString()
    {
        return $"({X}, {Y})";
    }

    public static IEnumerable<DataPoint> FromList(List<double> xData, List<double> yData)
    {
        if (xData.Count != yData.Count)
            throw new ArgumentException("X and Y data lists must have the same length.");

        for (int i = 0; i < xData.Count; i++)
        {
            yield return new DataPoint(xData[i], yData[i]);
        }
    }
}
