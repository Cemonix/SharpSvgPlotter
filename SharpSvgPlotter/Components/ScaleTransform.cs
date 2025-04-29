using System;
using SharpSvgPlotter.Primitives;
using SharpSvgPlotter.Components;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.Components;

public class ScaleTransform
{
    private readonly double _scaleX;
    private readonly double _scaleY;
    private readonly double _offsetX;
    private readonly double _offsetY;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ScaleTransform"/> class.
    /// Handles Y-axis inversion for SVG.
    /// </summary>
    /// <param name="xAxis">The configured X-axis.</param>
    /// <param name="yAxis">The configured Y-axis.</param>
    /// <param name="plotArea">The calculated plot area.</param>
    /// <returns>A configured ScaleTransform instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown if axis ranges or plot area dimensions are zero or invalid.</exception>
    public ScaleTransform(Axis xAxis, Axis yAxis, PlotArea plotArea)
    {
        ArgumentNullException.ThrowIfNull(plotArea);
        ArgumentNullException.ThrowIfNull(xAxis);
        ArgumentNullException.ThrowIfNull(yAxis);

        double dataWidth = xAxis.Max - xAxis.Min;
        double dataHeight = yAxis.Max - yAxis.Min;

        // --- Handle potential zero ranges ---
        if (Math.Abs(dataWidth) < Constants.Epsilon)
            throw new RangeException("X-axis range cannot be zero.", nameof(xAxis));
        if (Math.Abs(dataHeight) < Constants.Epsilon)
            throw new RangeException("Y-axis range cannot be zero.", nameof(yAxis));
        if (Math.Abs(plotArea.Width) < Constants.Epsilon || Math.Abs(plotArea.Height) < Constants.Epsilon)
            throw new ArgumentException("PlotArea dimensions cannot be zero.", nameof(plotArea));

        // Calculate scale factors (note Y inversion)
        _scaleX = plotArea.Width / dataWidth;
        _scaleY = -plotArea.Height / dataHeight;

        // Calculate offsets
        _offsetX = plotArea.X - xAxis.Min * _scaleX;
        _offsetY = plotArea.Y - yAxis.Max * _scaleY;
    }

    /// <summary>
    /// Transforms a data point from data space to pixel space.
    /// </summary>
    /// <param name="point">The data point to transform.</param>
    /// <returns>The transformed data point in pixel space.</returns>
    public DataPoint Transform(DataPoint point) => new (
        point.X * _scaleX + _offsetX, point.Y * _scaleY + _offsetY
    );

    /// <summary>
    /// Inversely transforms a data point from pixel space to data space.
    /// </summary>
    /// <param name="point">The pixel point to transform.</param>
    /// <returns>The inversely transformed data point in data space.</returns>
    public DataPoint InverseTransform(DataPoint point)
    {
        double invScaleX = Math.Abs(_scaleX) < Constants.Epsilon ? 0 : 1.0 / _scaleX;
        double invScaleY = Math.Abs(_scaleY) < Constants.Epsilon ? 0 : 1.0 / _scaleY;

        return new DataPoint((point.X - _offsetX) * invScaleX, (point.Y - _offsetY) * invScaleY);
    }
}
