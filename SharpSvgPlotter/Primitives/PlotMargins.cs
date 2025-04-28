namespace SharpSvgPlotter.Primitives;

public readonly struct PlotMargins(double left, double right, double top, double bottom)
{
    public double Left { get; init; } = left;
    public double Right { get; init; } = right;
    public double Top { get; init; } = top;
    public double Bottom { get; init; } = bottom;

    public override readonly string ToString()
    {
        return $"Left: {Left}, Right: {Right}, Top: {Top}, Bottom: {Bottom}";
    }
}
