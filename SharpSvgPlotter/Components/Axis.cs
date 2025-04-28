using System;
using SharpSvgPlotter.Series;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.Components;

public class Axis
{
    public double Min { get; private set; }
    public double Max { get; private set; }
    public string Label { get; set; } = string.Empty;
    public double Range => Max - Min;
    public bool AutoScale { get; set; } = true;

    public List<double> TickPositions { get; private set; } = [];
    public List<string> TickLabels { get; private set; } = [];

    public void SetRange(double min, double max)
    {
        if (max < min)
            throw new ArgumentException("Max must be greater than or equal to Min.");
        // TODO: Handle case where max == min? Maybe add tiny padding?
        Min = min;
        Max = max;
    }

    public void CalculateRange(IEnumerable<ISeries> series, AxisType axis_type)
    {
        if (!AutoScale)
            return;

        if (series == null || !series.Any())
            throw new ArgumentException("Series collection cannot be null or empty.");

        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (var s in series)
        {
            var (minX, maxX, minY, maxY) = s.GetDataRange();
            if (axis_type == AxisType.X)
            {
                min = Math.Min(min, minX);
                max = Math.Max(max, maxX);
            }
            else
            {
                min = Math.Min(min, minY);
                max = Math.Max(max, maxY);
            }
        }

        SetRange(min, max);
    }

    public void CalculateTicks()
    {
        TickPositions.Clear();
        TickLabels.Clear();

        if (Math.Abs(Range) < Constants.Epsilon) {
            throw new RangeException($"Range is too small to calculate ticks. Range: {Range}");
        }

        // --- Replace this with a better tick calculation algorithm ---
        // Simple Example (improvement over division, but still basic):
        int maxTicks = 10; // Desired max number of ticks
        double niceInterval = CalculateNiceInterval(Range, maxTicks);
        double firstTick = Math.Ceiling(Min / niceInterval) * niceInterval;
        double lastTick = Math.Floor(Max / niceInterval) * niceInterval;

        for (double currentTick = firstTick; currentTick <= Max + (niceInterval * 0.5); currentTick += niceInterval)
        {
             if (currentTick >= Min - (niceInterval * 0.5)) // Check bounds loosely
             {
                  TickPositions.Add(currentTick);
                  TickLabels.Add(currentTick.ToString("G3"));
             }
        }
        // --- End of simple example ---

        // Research "nice numbers algorithm C#" or "Wilkinson extended label algorithm C#" for robust implementations.
        throw new NotImplementedException("Robust tick calculation not implemented.");
    }

    private double CalculateNiceInterval(double range, int maxTicks)
    {
        double roughInterval = range / maxTicks;
        double exponent = Math.Floor(Math.Log10(roughInterval));
        double magnitude = Math.Pow(10, exponent);

        // Try intervals of 1, 2, 5 times the magnitude
        double[] niceMultipliers = { 1.0, 2.0, 5.0, 10.0 }; // 10 is fallback
        double bestInterval = magnitude * 10; // Default fallback

        foreach(double mult in niceMultipliers)
        {
            double currentInterval = magnitude * mult;
            if (range / currentInterval <= maxTicks + 1) // Check if tick count is reasonable
            {
                bestInterval = currentInterval;
                break;
            }
        }
        return bestInterval;
    }
}
