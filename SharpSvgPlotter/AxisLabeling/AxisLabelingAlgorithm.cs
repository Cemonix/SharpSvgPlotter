using System;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.AxisLabeling;

/// <summary>
/// Abstract base class for axis tick labeling algorithms.
/// Defines the contract for generating tick marks and labels for a given data range.
/// 
/// The primary source for these algorithms is the implementation in the R language: https://rdrr.io/cran/labeling/src/R/labeling.R
/// </summary>
public abstract class AxisLabelingAlgorithm
{
    /// <summary>
    /// Gets the name of the labeling algorithm.
    /// </summary>
    public abstract string AlgorithmName { get; }

    /// <summary>
    /// Generates tick positions and labels for a given data range and configuration.
    /// </summary>
    /// <param name="dataMin">The minimum value of the data range for the axis.</param>
    /// <param name="dataMax">The maximum value of the data range for the axis.</param>
    /// <param name="axisLength">The physical length of the axis (e.g., in pixels or inches) - needed for density/legibility.</param>
    /// <param name="options">Additional options to configure the algorithm.</param>
    /// <returns>A TickCalculationResult containing the generated ticks and labels, or null if no suitable labeling is found.</returns>
    public abstract TickCalculationResult? GenerateTicks(
        double dataMin, double dataMax, double axisLength, AxisLabelingOptions? options
    );

    /// <summary>
    /// Generates a list of tick positions and corresponding labels based on a calculated
    /// minimum, maximum, step size, and format string. Includes handling for edge cases.
    /// </summary>
    /// <param name="tickMin">The calculated starting value for ticks.</param>
    /// <param name="tickMax">The calculated ending value for ticks.</param>
    /// <param name="step">The calculated step size between ticks.</param>
    /// <param name="formatString">The format string for generating labels.</param>
    /// <returns>A tuple containing the list of tick positions and the list of tick labels.</returns>
    protected static (List<double> TickPositions, List<string> TickLabels) GenerateTicksFromRangeStep(
        double tickMin, double tickMax, double step, string formatString)
    {
        List<double> tickPositions = [];
        List<string> tickLabels = [];

        // Ensure step is valid to prevent infinite loops
        if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
        {
            Console.Error.WriteLine($"Error: Invalid step ({step}) in GenerateTicksFromRangeStep. Returning single tick.");
            // Handle by returning just the min tick if possible, or just 0
            double singleTick = !double.IsNaN(tickMin) && !double.IsInfinity(tickMin) ? tickMin : 0;
            tickPositions.Add(singleTick);
            tickLabels.Add(singleTick.ToString(formatString));
            return (tickPositions, tickLabels);
        }


        // Add a small epsilon to the end condition for floating point inaccuracies
        double endValue = tickMax + (step * Constants.Epsilon);
        int safetyCounter = 0;
        const int maxIterations = 1000; // Safety limit

        for (double currentTick = tickMin; currentTick <= endValue; currentTick += step)
        {
            // Check for duplicate ticks due to precision errors, especially with very small steps
            if (tickPositions.Count > 0 && Math.Abs(currentTick - tickPositions[^1]) < step * Constants.Epsilon) 
                continue;

            tickPositions.Add(currentTick);
            tickLabels.Add(currentTick.ToString(formatString));

            safetyCounter++;
            if (safetyCounter > maxIterations) // Safety break
            {
                Console.Error.WriteLine(
                    $"Warning: Tick generation exceeded {maxIterations} iterations. Breaking loop. Step='{step}'"
                );
                break;
            }
        }

        // --- Handle edge cases where loop might not generate enough ticks ---

        // Ensure at least one tick if min/max were valid
        if (tickPositions.Count == 0 && !double.IsNaN(tickMin) && !double.IsInfinity(tickMin))
        {
            tickPositions.Add(tickMin);
            tickLabels.Add(tickMin.ToString(formatString));
        }

        // Ensure at least two ticks if the calculated range implies it should be possible
        // This also covers the case where the loop added only tickMin.
        if (tickPositions.Count < 2 && Math.Abs(tickMin - tickMax) > step * Constants.Epsilon)
        {
            // Check if tickMax is distinct enough from the last added tick (which is tickMin if count is 1)
            if (Math.Abs(tickPositions[^1] - tickMax) > step * Constants.Epsilon)
            {
                tickPositions.Add(tickMax);
                tickLabels.Add(tickMax.ToString(formatString));
            }
        }

        return (tickPositions, tickLabels);
    }
}
