using System;

namespace SharpSvgPlotter.Primitives;

public class PlotArea(double width, double height, PlotMargins margins)
{
    private readonly DataPoint _origin = new (margins.Left, margins.Top);
    private readonly double _width = width - margins.Left - margins.Right;
    private readonly double _height = height - margins.Top - margins.Bottom;

    public PlotMargins Margins => margins;
    public double X => _origin.X;
    public double Y => _origin.Y;
    public double Width => _width; 
    public double Height => _height;
    public double Area => _width * _height;
}
