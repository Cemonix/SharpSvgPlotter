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
}
