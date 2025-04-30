using System;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.AxisLabeling;

/// <summary>
/// Implements the tick labeling algorithm used by Gnuplot.
/// Based on the R implementation found in the 'labeling' package:
/// https://rdrr.io/cran/labeling/src/R/labeling.R
/// </summary>
internal class GnuPlotAlgorithm : AxisLabelingAlgorithm
{
    internal override string AlgorithmName => "GnuPlot";

    internal override TickCalculationResult? GenerateTicks(
        double dataMin, double dataMax, double axisLength, AxisLabelingOptions? options)
    {
        // --- Input Validation ---
        if (dataMin > dataMax)
        {
            throw new ArgumentException("dataMin must be less than or equal to dataMax.");
        }
        if (axisLength <= 0)
        {
            throw new ArgumentException("axisLength must be positive.");
        }

        double range = dataMax - dataMin;
        string formatString = options?.FormatString ?? "G3"; // Default format string
        int m = options?.TickCount ?? 5; // Default to 5 ticks
        if (m < 2) m = 2; // Need at least 2 ticks for a step
        int ntick = m; // Gnuplot implementation uses floor, but let's stick to 'm' for consistency unless issues arise.

        // Handle zero or near-zero range
        if (range < Constants.Epsilon)
        {
            // Return a single tick at the value if min == max
            var positions = new List<double> { dataMin };
            var labels = new List<string> { dataMin.ToString(formatString) };
            return new TickCalculationResult(positions, labels, dataMin, dataMax);
        }

        // --- Algorithm Steps ---

        // Calculate power of 10 just below the range magnitude
        double power = Math.Pow(10, Math.Floor(Math.Log10(range)));

        // Normalize the range (should be roughly between 1 and 10)
        double norm_range = range / power;
        if (Math.Abs(norm_range) < Constants.Epsilon)
        {
            // This case should ideally be caught by the initial range check, but as a safeguard:
            norm_range = Constants.Epsilon; // Avoid division by zero
            Console.Error.WriteLine($"Warning: Gnuplot norm_range was zero. Range: {range}, Power: {power}");
        }

        // Intermediate calculation 'p'
        double p = (ntick - 1) / norm_range;

        // Select tick interval multiplier 't' based on 'p'
        double t;
        if (p > 40)       t = 0.05;
        else if (p > 20)  t = 0.1;
        else if (p > 10)  t = 0.2;
        else if (p > 4)   t = 0.5;
        else if (p > 2)   t = 1;
        else if (p > 0.5) t = 2;
        else              t = Math.Ceiling(norm_range); // Fallback

        // Calculate the actual tick step size 'd'
        double d = t * power;
        if (d <= 0 || double.IsNaN(d) || double.IsInfinity(d))
        {
            // Fallback if step calculation fails unexpectedly
            d = range / (m > 1 ? m-1 : 1); // Simple division as fallback
            d = d > Constants.Epsilon ? d : Constants.Epsilon;
            Console.Error.WriteLine($"Warning: Gnuplot step 'd' was invalid ({t*power}). Using fallback: {d}");
        }

        // Calculate the first tick position (graph minimum for labels)
        double graphMin = Math.Floor(dataMin / d) * d;

        // Calculate the last tick position (or one beyond, graph maximum for labels)
        double graphMax = Math.Ceiling(dataMax / d) * d;

        // --- Generate Ticks ---
        (List<double> tickPositions, List<string> tickLabels) = GenerateTicksFromRangeStep(
            graphMin, graphMax, d, formatString
        );

        double actualMin = tickPositions[0];
        double actualMax = tickPositions[^1];

        return new TickCalculationResult(tickPositions, tickLabels, actualMin, actualMax);
    }
}
