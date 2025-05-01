using System;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.Series.HistogramSupport;

internal static class HistogramBinner
{
    /// <summary>
    /// Generates histogram bins based on the provided data and options.
    /// </summary>
    /// <param name="data">The raw numerical data.</param>
    /// <param name="mode">The binning mode (Automatic, ManualCount, ManualWidth).</param>
    /// <param name="manualBinCount">The desired number of bins if mode is ManualCount.</param>
    /// <param name="manualBinWidth">The desired bin width if mode is ManualWidth.</param>
    /// <param name="autoRule">The rule to use if mode is Automatic.</param>
    /// <returns>A list of HistogramBin objects.</returns>
    /// <exception cref="ArgumentException">Thrown if data is null or empty, or if manual parameters are invalid.</exception>
    internal static List<HistogramBin> GenerateBins(
        IEnumerable<double> data,
        HistogramBinningMode mode,
        int? manualBinCount,
        double? manualBinWidth,
        AutomaticBinningRule autoRule
    ) {
        ArgumentNullException.ThrowIfNull(data);
        var dataList = data.Where(d => !double.IsNaN(d) && !double.IsInfinity(d)).ToList();
        if (dataList.Count == 0) return []; // Return empty list if no valid data

        double dataMin = dataList.Min();
        double dataMax = dataList.Max();
        double dataRange = dataMax - dataMin;
        int N = dataList.Count;

        // Handle zero range data
        if (dataRange < Constants.Epsilon)
        {
            // Create a single bin centered around the value
            double width = 1.0;
            double lower = dataMin - width / 2.0;
            double upper = dataMin + width / 2.0;
            return [new HistogramBin(lower, upper, N)];
        }

        int numberOfBins;
        double binWidth;

        // --- Determine Bin Width and Count based on Mode ---
        switch (mode)
        {
            case HistogramBinningMode.ManualCount:
                if (manualBinCount == null || manualBinCount.Value <= 0)
                    throw new ArgumentException("ManualBinCount must be positive when mode is ManualCount.", nameof(manualBinCount));
                numberOfBins = manualBinCount.Value;
                binWidth = dataRange / numberOfBins;
                break;

            case HistogramBinningMode.ManualWidth:
                if (manualBinWidth == null || manualBinWidth.Value <= Constants.Epsilon)
                    throw new ArgumentException("ManualBinWidth must be positive when mode is ManualWidth.", nameof(manualBinWidth));
                binWidth = manualBinWidth.Value;
                // Calculate number of bins needed (ensure it covers the range)
                numberOfBins = (int)Math.Ceiling(dataRange / binWidth);
                // Prevent zero bins if range is tiny compared to width
                if (numberOfBins == 0) numberOfBins = 1;
                break;

            case HistogramBinningMode.Automatic:
            default: // Default to Automatic
                (binWidth, numberOfBins) = CalculateAutomaticBins(dataList, autoRule, dataRange, N);
                break;
        }

        // Ensure bin width is reasonable
        if (binWidth <= Constants.Epsilon)
        {
            // Fallback if calculated width is too small (e.g., from automatic rules with odd data)
            Console.Error.WriteLine($"Warning: Calculated histogram bin width ({binWidth}) was near zero. Falling back to 10 bins.");
            numberOfBins = 10;
            binWidth = dataRange / numberOfBins;
            if (binWidth <= Constants.Epsilon) binWidth = Constants.Epsilon; // Final safety
        }

        // --- Create Bin Edges ---
        // Adjust min slightly so exact min falls in first bin using '<' comparison later
        double firstBinLowerBound = dataMin;
        var binEdges = new List<double>();
        for (int i = 0; i <= numberOfBins; i++)
        {
            binEdges.Add(firstBinLowerBound + i * binWidth);
        }
        // Ensure last edge covers max if rounding caused issues (though Ceiling should handle it)
        if (binEdges[^1] < dataMax - Constants.Epsilon) {
            binEdges[^1] = dataMax;
        }


        // --- Create Bins and Count Data ---
        var bins = new List<HistogramBin>(numberOfBins);
        var counts = new int[numberOfBins];

        foreach (double value in dataList)
        {
            // Find the correct bin index
            int binIndex = -1;
            for (int i = 0; i < numberOfBins; i++)
            {
                // Use [lower, upper) logic
                if (value >= binEdges[i] - Constants.Epsilon && value < binEdges[i + 1] - Constants.Epsilon)
                {
                    binIndex = i;
                    break;
                }
            }
            // Special case for the maximum value to fall into the last bin
            if (binIndex == -1 && Math.Abs(value - binEdges[^1]) < Constants.Epsilon)
                binIndex = numberOfBins - 1;

            if (binIndex >= 0 && binIndex < numberOfBins)
                counts[binIndex]++;

            // else: Value might be outside calculated edges due to floating point issues
            else if (value < binEdges[0] || value > binEdges[^1])
            {
                Console.Error.WriteLine(
                    $"Warning: Histogram data point {value} fell outside calculated bin edges " +
                    $"[{binEdges[0]}, {binEdges[^1]}]. This may indicate an issue with the data or binning parameters."
                );
            }
        }

        // --- Populate the final Bin list ---
        for (int i = 0; i < numberOfBins; i++)
        {
            bins.Add(new HistogramBin(binEdges[i], binEdges[i + 1], counts[i]));
        }

        return bins;
    }

    /// <summary>
    /// Calculates the bin width and number of bins based on the selected automatic binning rule.
    /// This method assumes the data is already sorted for efficiency.
    /// </summary>
    /// <param name="sortedData">The sorted data points.</param>
    /// <param name="rule">The automatic binning rule to apply.</param>
    /// <param name="dataRange">The range of the data (max - min).</param>
    /// <param name="N">The number of data points.</param>
    /// <returns>A tuple containing the bin width and number of bins.</returns>
    private static (double BinWidth, int NumberOfBins) CalculateAutomaticBins(
        List<double> sortedData,
        AutomaticBinningRule rule,
        double dataRange,
        int N
    ) {
        double binWidth;
        int numberOfBins;

        switch (rule)
        {
            case AutomaticBinningRule.Sturges:
                numberOfBins = (int)Math.Ceiling(Math.Log2(N) + 1);
                binWidth = dataRange / numberOfBins;
                break;

            case AutomaticBinningRule.Scott:
                double stdDev = CalculateStandardDeviation(sortedData);
                binWidth = 3.49 * stdDev / Math.Pow(N, 1.0 / 3.0);
                numberOfBins = (int)Math.Ceiling(dataRange / binWidth);
                break;

            case AutomaticBinningRule.FreedmanDiaconis:
                double iqr = CalculateIQR(sortedData);
                // Handle zero IQR case (e.g., constant data subset)
                if (iqr < Constants.Epsilon) {
                   // Fallback to square root choice
                   goto case AutomaticBinningRule.SquareRoot;
                }
                binWidth = 2.0 * iqr / Math.Pow(N, 1.0 / 3.0);
                numberOfBins = (int)Math.Ceiling(dataRange / binWidth);
                break;

            case AutomaticBinningRule.SquareRoot:
            default: // Default to SquareRoot
                numberOfBins = (int)Math.Ceiling(Math.Sqrt(N));
                binWidth = dataRange / numberOfBins;
                break;
        }

        // Safety checks
        if (numberOfBins <= 0) numberOfBins = 1;
        if (binWidth <= Constants.Epsilon) 
            binWidth = dataRange > Constants.Epsilon ? dataRange / numberOfBins : Constants.Epsilon;

        return (binWidth, numberOfBins);
    }

    // --- Statistics Helpers ---

    /// <summary>
    /// Calculates the standard deviation of a list of doubles.
    /// </summary>
    /// <param name="data">The data points.</param>
    /// <returns>The standard deviation.</returns>
    private static double CalculateStandardDeviation(List<double> data)
    {
        int n = data.Count;
        if (n < 2) return 0;
        double mean = data.Average();
        double sumSquaredDiffs = data.Sum(d => (d - mean) * (d - mean));
        return Math.Sqrt(sumSquaredDiffs / (n - 1));
    }

    /// <summary>
    /// Calculates the Interquartile Range (IQR) of a list of doubles.
    /// </summary>
    /// <param name="data">The data points.</param>
    /// <returns>The IQR.</returns>
    private static double CalculateIQR(List<double> data)
    {
        // Ensure data is sorted for percentile calculation
        var sortedData = data.OrderBy(d => d).ToList();
        int n = sortedData.Count;
        if (n < 2) return 0; // Need at least 2 points for IQR

        double q1 = GetPercentile(sortedData, 25);
        double q3 = GetPercentile(sortedData, 75);
        return q3 - q1;
    }

    /// <summary>
    /// Calculates the nth percentile of a sorted list of doubles.
    /// </summary>
    /// <param name="sortedData">The sorted data points.</param>
    /// <param name="percentile">The desired percentile (0-100).</param>
    /// <returns>The value at the specified percentile.</returns>
    private static double GetPercentile(List<double> sortedData, double percentile)
    {
        int n = sortedData.Count;
        if (n == 0) return double.NaN;
        if (n == 1) return sortedData[0];
        if (percentile <= 0) return sortedData[0];
        if (percentile >= 100) return sortedData[n - 1];

        // Calculate index (0-based)
        double index = percentile / 100.0 * (n - 1);
        int lowerIndex = (int)Math.Floor(index);
        double fraction = index - lowerIndex;

        // Should not happen with checks above
        if (lowerIndex >= n - 1) return sortedData[n - 1]; 

        // Linear interpolation
        return sortedData[lowerIndex] + fraction * (sortedData[lowerIndex + 1] - sortedData[lowerIndex]);
    }
}
