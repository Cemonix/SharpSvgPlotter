using System;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.AxisLabeling;

public class HeckBertAlgorithm : AxisLabelingAlgorithm
{
    public override string AlgorithmName => "HeckBert";

    public override TickCalculationResult? GenerateTicks(
        double dataMin, double dataMax, double axisLength, AxisLabelingOptions? options
    ) {
        if (dataMin > dataMax) 
        {
            // Handle zero range: maybe return a single tick?
            if (Math.Abs(dataMin - dataMax) < 1e-10) {
                var pos = new List<double> { dataMin };
                var lab = new List<string> { dataMin.ToString("G3") };
                return new TickCalculationResult(pos, lab, dataMin, dataMax);
            }
            throw new ArgumentException("dataMin must be less than or equal to dataMax.");
        }

        if (axisLength <= 0) {
            throw new ArgumentException("axisLength must be positive.");
        }

        string formatString = options?.FormatString ?? "G3"; // Default format string

        int m = options?.TickCount ?? 5; // Default to 5 ticks if not specified
        if (m < 2) m = 2; // Need at least 2 ticks for a step
        int intervals = m - 1;

        // --- Algorithm Steps ---

        // Calculate the nice range and step
        double range = HeckBertNiceNum(dataMax - dataMin, false);

        // Ensure range is not zero if dataMin != dataMax
        if (range < Constants.Epsilon && (dataMax - dataMin) > Constants.Epsilon) {
            // Fallback if nicenum returns zero unexpectedly for non-zero range
            range = dataMax - dataMin;
        }
        if (range < Constants.Epsilon) // Avoid division by zero if dataMin==dataMax resulted in ~0 range
            range = Constants.Epsilon;

        double l_step = HeckBertNiceNum(range / intervals, true);
        if (l_step <= 0) {
            // Fallback if step is zero (e.g., due to very small range / large m)
            l_step = Math.Abs(dataMax-dataMin) > Constants.Epsilon ? (dataMax-dataMin) : Constants.Epsilon;
            Console.Error.WriteLine($"Warning: Heckbert NiceNum step was zero. Using fallback: {l_step}");
        }

        // Calculate nice min and max label values
        double l_min = Math.Floor(dataMin / l_step) * l_step;
        double l_max = Math.Ceiling(dataMax / l_step) * l_step;

        // --- Generate Ticks ---
        (List<double> tickPositions, List<string> tickLabels) = GenerateTicksFromRangeStep(
            l_min, l_max, l_step, formatString
        );

        double actualMin = tickPositions[0];
        double actualMax = tickPositions[^1];

        return new TickCalculationResult(tickPositions, tickLabels, actualMin, actualMax);
    }

    /// <summary>
    /// Calculates a "nice" number based on the range and whether to round it.
    /// This is a helper function used in the Heck-Bert algorithm to determine the step size for tick marks.
    /// </summary>
    /// <param name="range">The range of values to be covered by the tick marks.</param>
    /// <param name="round">If true, the function will round the result to a "nice" number.</param>
    /// <returns>A "nice" number that is a multiple of 1, 2, or 5, adjusted to the range.</returns>
    private static double HeckBertNiceNum(double range, bool round)
    {
        double exponent = Math.Floor(Math.Log10(range));
        double fraction = range / Math.Pow(10, exponent);
        double niceFraction;

        if (round) {
            if (fraction < 1.5) niceFraction = 1;
            else if (fraction < 3) niceFraction = 2;
            else if (fraction < 7) niceFraction = 5;
            else niceFraction = 10;
        } else {
            if (fraction <= 1) niceFraction = 1;
            else if (fraction <= 2) niceFraction = 2;
            else if (fraction <= 5) niceFraction = 5;
            else niceFraction = 10;
        }

        return niceFraction * Math.Pow(10, exponent);
    }
}
