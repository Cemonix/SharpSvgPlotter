using System;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.AxisLabeling;

/// <summary>
/// Implements the tick labeling algorithm used by Matplotlib (Python library).
/// Based on the R implementation found in the 'labeling' package:
/// https://rdrr.io/cran/labeling/src/R/labeling.R
/// </summary>
internal class MatplotlibAlgorithm : AxisLabelingAlgorithm
{
    internal override string AlgorithmName => "Matplotlib";

    // Predefined "nice" step multipliers
    private static readonly double[] _steps = [1.0, 2.0, 5.0, 10.0];
    private static readonly double _threshold = 100.0; // Threshold for offset calculation
    private const bool _trim = true; // Controls whether to trim extra bins extending beyond data max

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

        string formatString = options?.FormatString ?? "G3";
        int m = options?.TickCount ?? 5;
        if (m < 2) m = 2;
        int nbins = m; // Matplotlib uses 'nbins' which corresponds to desired ticks 'm'

        // Handle zero or near-zero range immediately
        if (Math.Abs(dataMax - dataMin) < Constants.Epsilon)
        {
            var positions = new List<double> { dataMin };
            var labels = new List<string> { dataMin.ToString(formatString) };
            return new TickCalculationResult(positions, labels, dataMin, dataMax);
        }

        // --- Algorithm Steps ---

        // 1. Calculate Scale and Offset using helper
        (double scale, double offset) = ScaleRange(dataMin, dataMax, nbins);

        // 2. Adjust range by offset
        double vmin = dataMin - offset;
        double vmax = dataMax - offset;
        double adjustedRange = vmax - vmin;

        // Handle case where adjusted range is zero (can happen if offset dominates)
        if (Math.Abs(adjustedRange) < Constants.Epsilon)
        {
            // Treat as zero-range case centered at the adjusted value (which is close to zero)
            double centerVal = dataMin - offset; // or dataMax - offset
            var positions = new List<double> { centerVal + offset }; // Add offset back
            var labels = new List<string> { positions[0].ToString(formatString) };
            return new TickCalculationResult(positions, labels, positions[0], positions[0]);
        }

        // 3. Calculate raw and scaled steps
        double rawStep = adjustedRange / nbins;
        double scaledRawStep = rawStep / scale;

        // 4. Find best "nice" step
        double bestMin = vmin; // Initialize with adjusted values
        double bestMax = vmax;
        double scaledStep = scale; // Default to scale if no step >= scaledRawStep found

        foreach (double stepFactor in _steps)
        {
            if (stepFactor >= scaledRawStep)
            {
                scaledStep = stepFactor * scale;
                // Avoid zero step
                if (Math.Abs(scaledStep) < Constants.Epsilon) {
                    scaledStep = Constants.Epsilon; // Use tiny step as fallback
                    Console.Error.WriteLine(
                        "Warning: Matplotlib scaledStep became zero " +
                        $"(factor={stepFactor}, scale={scale}). Using epsilon."
                    );
                }

                bestMin = scaledStep * Math.Floor(vmin / scaledStep);
                bestMax = bestMin + scaledStep * nbins; // Max calculated based on number of bins

                // If this range covers the adjusted max, we found our step
                if (bestMax >= vmax - (scaledStep * Constants.Epsilon)) // Add epsilon tolerance
                    break;
            }
        }

        // 5. Optional Trimming (if bestMax extends beyond vmax)
        if (_trim && scaledStep > Constants.Epsilon)
        {
            // Calculate how many bins are fully beyond vmax
            double overhang = bestMax - vmax;
            if (overhang > Constants.Epsilon) // Only trim if there's a noticeable overhang
            {
                int extraBins = (int)Math.Floor(overhang / scaledStep);
                if (extraBins > 0 && (nbins - extraBins >= 1)) // Ensure at least one bin remains
                {
                    nbins -= extraBins;
                    // Recalculate bestMax based on trimmed nbins
                    bestMax = bestMin + scaledStep * nbins;
                }
            }
        }

        // 6. Final Graph Min/Max (add offset back)
        double graphMin = bestMin + offset;
        double graphMax = bestMin + nbins * scaledStep + offset; // Use trimmed nbins here

        // --- Generate Ticks ---
        (List<double> tickPositions, List<string> tickLabels) = GenerateTicksFromRangeStep(
            graphMin, graphMax, scaledStep, formatString
        );

        double actualMin = tickPositions[0];
        double actualMax = tickPositions[^1];

        return new TickCalculationResult(tickPositions, tickLabels, actualMin, actualMax);
    }

    /// <summary>
    /// Helper function corresponding to Matplotlib's _scale_range.
    /// Calculates a scale factor and offset for the axis range.
    /// </summary>
    private (double scale, double offset) ScaleRange(double dmin, double dmax, int nbins)
    {
        double dv = Math.Abs(dmax - dmin);
        double maxabs = Math.Max(Math.Abs(dmin), Math.Abs(dmax));

        // Handle zero range or very small range relative to magnitude
        if (maxabs < Constants.Epsilon || dv / maxabs < Constants.Epsilon)
        {
            return (1.0, 0.0); // Default scale=1, offset=0
        }

        double meanv = 0.5 * (dmin + dmax);
        double offset = 0.0;

        // Determine offset (only if mean is large relative to range)
        if (Math.Abs(meanv) / dv >= _threshold)
        {
            if (meanv > 0)
            {
                double exp = Math.Floor(Math.Log10(meanv));
                offset = Math.Pow(10.0, exp);
            }
            else
            {
                double exp = Math.Floor(Math.Log10(-meanv));
                offset = -Math.Pow(10.0, exp);
            }
        }

        // Determine scale based on range and bins
        double scale;
        double valForLog = dv / Math.Max(1, nbins); // Use Max(1, nbins) to avoid issues if nbins=0 passed somehow
        if (valForLog > Constants.Epsilon)
        {
            double expScale = Math.Floor(Math.Log10(valForLog));
            scale = Math.Pow(10.0, expScale);
        }
        else
        {
            scale = 1.0; // Fallback if range/nbins is too small
            Console.Error.WriteLine($"Warning: Matplotlib scale calculation input was too small ({valForLog}). Using scale=1.0");
        }


        return (scale, offset);
    }
}
