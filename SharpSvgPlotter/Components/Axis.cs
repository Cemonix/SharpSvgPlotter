using System;
using SharpSvgPlotter.AxisLabeling;
using SharpSvgPlotter.Series;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.Components;

internal class Axis(
    string label,
    AxisLabelingAlgorithm labelingAlgorithm,
    AxisLabelingOptions? labelingOptions = null,
    bool autoScale = true
) {
    public double Min { get; private set; }
    public double Max { get; private set; }
    public string Label { get; init; } = label ?? string.Empty;
    public bool AutoScale { get; init; } = autoScale;

    internal AxisLabelingAlgorithm LabelingAlgorithm { get; init; } = labelingAlgorithm ?? new HeckBertAlgorithm();
    internal AxisLabelingOptions LabelingOptions { get; init; } = labelingOptions ?? new(); // Default options

    internal List<double> TickPositions { get; private set; } = [];
    internal List<string> TickLabels { get; private set; } = [];

    /// <summary>
    /// Calculates the range of the axis based on the provided series data.
    /// </summary>
    /// <param name="series">The series data to calculate the range from.</param>
    /// <param name="axisType">The type of axis (X or Y).</param>
    /// <remarks>
    internal void CalculateRange(IEnumerable<ISeries> series, AxisType axisType)
    {
        if (!AutoScale)
            return;

        if (series == null || !series.Any())
        {
            // Set a default range if no series data provided for autoscaling
            SetRange(0, 1);
            Console.Error.WriteLine(
                $"Warning: AutoScale enabled for {axisType}-Axis but no series data provided. Defaulting range to [0, 1]."
            );
            return;
        }

        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (var s in series)
        {
            (double sMin, double sMax) = s.GetAxisBounds(axisType);

            // Check if valid bounds were returned
            if (!double.IsNaN(sMin) && !double.IsNaN(sMax))
            {
                min = Math.Min(min, sMin);
                max = Math.Max(max, sMax);
            }
        }

        // Handle case where no series had valid data points
        if (min == double.MaxValue || max == double.MinValue)
        {
            SetRange(0, 1);
            Console.Error.WriteLine(
                $"Warning: AutoScale enabled for {axisType}-Axis but no valid data points found in series. "
                + "Defaulting range to [0, 1]."
            );
            return;
        }

        // Special case for Y-axis with histogram series: ensure min is at least 0
        if (axisType == AxisType.Y && series.Any(s => s is HistogramSeries))
        {
            min = Math.Min(min, 0);
        }

        // Add padding (only if range is not zero)
        double range = max - min;
        double padding = 0;
        if (range > Constants.Epsilon) {
            padding = range * 0.05; // 5% padding
        } else {
            padding = (Math.Abs(min) > Constants.Epsilon) ? Math.Abs(min) * 0.1 : 0.5; // 10% of value or 0.5 default
        }

        double finalMin = min - padding;
        double finalMax = max + padding;

        // Ensure Y-axis doesn't go unnecessarily below zero after padding if forced
        if (axisType == AxisType.Y && finalMin < 0 && min >= 0)
        {
            finalMin = 0;
        }

        SetRange(finalMin, finalMax);

        TickPositions.Clear();
        TickLabels.Clear();
    }

    /// <summary>
    /// Calculates tick positions and labels using the assigned LabelingAlgorithm.
    /// </summary>
    /// <param name="axisLength">The physical length of the axis in the output medium (e.g., pixels).
    /// Required by some algorithms for density calculations.</param>
    internal void CalculateTicks(double axisLength)
    {
        TickPositions.Clear();
        TickLabels.Clear();

        // Ensure the range is valid before calling the algorithm
        if (Math.Abs(Max - Min) < Constants.Epsilon)
        {
            // Handle zero-range axis: generate a single tick
            double centerTick = Min;
            TickPositions.Add(centerTick);
            TickLabels.Add(centerTick.ToString(LabelingOptions.FormatString ?? "G3"));
            return;
        }

        // Generate ticks using the selected algorithm
        TickCalculationResult? result = LabelingAlgorithm.GenerateTicks(Min, Max, axisLength, LabelingOptions);

        if (result != null && result.TickPositions.Count > 0)
        {
            TickPositions = result.TickPositions;
            TickLabels = result.TickLabels;

            SetRange(result.ActualMin, result.ActualMax);
        }
        else
        {
            // Handle case where algorithm failed or returned no ticks (should be rare)
            Console.Error.WriteLine(
                $"Warning: Labeling algorithm '{LabelingAlgorithm.AlgorithmName}' failed " +
                $"to generate ticks for range [{Min}, {Max}].");

            TickPositions.Add(Min);
            TickLabels.Add(Min.ToString(LabelingOptions.FormatString ?? "G3"));
            if (Math.Abs(Max - Min) > Constants.Epsilon) // Avoid duplicate if Min==Max
            {
                TickPositions.Add(Max);
                TickLabels.Add(Max.ToString(LabelingOptions.FormatString ?? "G3"));
            }
        }
    }

    private void SetRange(double min, double max)
    {
        if (max < min)
            throw new ArgumentException("Max must be greater than or equal to Min.");
        if (Math.Abs(max - min) < Constants.Epsilon)
        {
            double padding = Constants.Epsilon;
            if (max == min)
            {
                max += padding / 2;
                min -= padding / 2;
            }
            else if (max > min)
            {
                max += padding;
            }
            else
            {
                min -= padding;
            }
        }
        if (double.IsInfinity(min) || double.IsInfinity(max))
            throw new ArgumentException("Min and Max cannot be set to infinity.");
        if (double.IsNaN(min) || double.IsNaN(max))
            throw new ArgumentException("Min and Max cannot be NaN.");
            
        Min = min;
        Max = max;
    }
}
