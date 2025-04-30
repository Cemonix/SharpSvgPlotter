using System;
using SharpSvgPlotter.Utils;

namespace SharpSvgPlotter.AxisLabeling;

// TODO: Not finished - probably overkill for first versions of the library
// Paper - An Extension of Wilkinson’s Algorithm for Positioning Tick Labels on Axes
// https://rdrr.io/cran/labeling/src/R/labeling.R
internal class WilkinsonExtended
{
    private static readonly float[] Q = [1, 5, 2, 2.5f, 4, 3];
    private static readonly float[] w = [0.2f, 0.25f, 0.5f, 0.05f];

    internal static TickLabelingResult? GenerateLabels(
        double rangeMin, double rangeMax, double ro_t, int max_iterations
    ) {
        throw new NotImplementedException("GenerateLabels not implemented.");

        // TickLabelingResult? result = null;
        // double best_score = -2;

        // for (int j = 1; ; j++) {
        //     foreach (double q in Q) {
        //         double sim_max = SimplicityMax(q, j);
        //         if ((w[0] * sim_max) + (w[1] * 1) + (w[2] * 1) + (w[3] * 1) < best_score)
        //             break;

        //         int k = 2;
        //         while (true) {
        //             double den_max = DensityMax(k);
        //             if ((w[0] * sim_max) + (w[1] * 1) + (w[2] * den_max) + (w[3] * 1) < best_score)
        //                 break;

        //             double delta = (rangeMax - rangeMin) / (k + 1) / (j * q);
        //             if (delta <= 0) continue; // Avoid log of non-positive

        //             for (double z = Math.Floor(Math.Log10(delta)); ; z++) {
        //                 double l_step = q * j * Math.Pow(10, z);
        //                 if (l_step <= 0) continue; // Safety check

        //                 double cov_max = CoverageMax(rangeMin, rangeMax, k - 1, l_step);
        //                 if ((w[0] * sim_max) + (w[1] * cov_max) + (w[2] * den_max) + (w[3] * 1) < best_score)
        //                     break;

        //                 double from = Math.Floor(rangeMax / l_step) - (k - 1);
        //                 double to = Math.Ceiling(rangeMin / l_step);
        //                 double step = 1 / j;
        //                 IEnumerable<double> series = Range.GetRange(from, to, step);
        //                 foreach (double start in series) {
        //                     double l_min = start * l_step;
        //                     double l_max = l_min + (k - 1) * l_step;
        //                     double sim = Simplicity(q, j, l_min, l_max, l_step);
        //                     double den = Density(l_min, l_max, l_step);
        //                     double cov = Coverage(l_min, l_max, l_step);

        //                     if ((w[0] * sim) + (w[1] * cov) + (w[2] * den) + (w[3] * 1) < best_score)
        //                         continue;
                            
        //                     (l, l_format) = OptLegibility(l_min, l_max, l_step);

        //                     double score = (w[0] * sim) + (w[1] * cov) + (w[2] * den) + (w[3] * l);
        //                     if (score > best_score) {
        //                         best_score = score;
        //                         result = Label(l_min, l_max, l_step, l_format);
        //                     }
        //                 }
        //             }

        //             k++;
        //         }
        //     }

        //     if (j > max_iterations)
        //         break;
        // }

        // return result;
    }

    /// <summary>
    /// Calculates the Simplicity score for a given labeling parameters.
    /// Based on formula: 1 - (i-1)/(|Q|-1) - j + v
    /// </summary>
    /// <param name="q">The base nice number used.</param>
    /// <param name="j">The skip amount used.</param>
    /// <param name="l_min">The minimum label value in the sequence.</param>
    /// <param name="l_max">The maximum label value in the sequence.</param>
    /// <param name="l_step">The step size between labels.</param>
    /// <returns>The simplicity score.</returns>
    private static double Simplicity(double q, int j, double l_min, double l_max, double l_step)
    {
        // 1. Find index 'i' of q in Q (1-based index)
        int i = -1;
        for (int idx = 0; idx < Q.Length; idx++)
        {
            if (Math.Abs(Q[idx] - q) < Constants.Epsilon)
            {
                i = idx + 1;
                break;
            }
        }

        if (i == -1)
            throw new ArgumentException($"Base nice number q={q} not found in Q.", nameof(q));

        // 2. Determine 'v' (whether 0 is included and reachable)
        int v = 0;
        if (l_min <= 0 && l_max >= 0 && l_step > Constants.Epsilon) // Check if 0 is within the range
        {
            // Check if 0 is actually reachable by stepping from l_min
            if (
                Math.Abs(l_min % l_step) < Constants.Epsilon || 
                Math.Abs((l_min % l_step) - l_step) < Constants.Epsilon
            ) {
                v = 1;
            }
        }

        // 3. Calculate score
        // Ensure denominator |Q|-1 is not zero if Q has only 1 element
        double penalty_i = (Q.Length > 1) ? (i - 1.0) / (Q.Length - 1.0) : 0;
        return 1.0 - penalty_i - j + v;
    }

    private double Density(double l_min, double l_max, double l_step)
    {
        throw new NotImplementedException("Density not implemented.");
    }

    /// <summary>
    /// Calculates the Coverage score for a given labeling relative to the data range.
    /// Based on formula: 1 - 0.5 * ( (dmax-lmax)^2 + (dmin-lmin)^2 ) / (0.1 * (dmax-dmin))^2
    /// </summary>
    /// <param name="d_min">The minimum value of the data.</param>
    /// <param name="d_max">The maximum value of the data.</param>
    /// <param name="l_min">The minimum label value in the sequence.</param>
    /// <param name="l_max">The maximum label value in the sequence.</param>
    /// <returns>The coverage score.</returns>
    private static double Coverage(double d_min, double d_max, double l_min, double l_max)
    {
        double data_range = d_max - d_min;

        // Handle zero data range case to avoid division by zero
        if (Math.Abs(data_range) < Constants.Epsilon)
        {
            return (
                Math.Abs(l_min - d_min) < Constants.Epsilon && 
                Math.Abs(l_max - d_max) < Constants.Epsilon
            ) ? 1.0 : 0.0;
        }

        double term1 = Math.Pow(d_max - l_max, 2);
        double term2 = Math.Pow(d_min - l_min, 2);
        double denominator = Math.Pow(0.1 * data_range, 2);

        // Avoid division by zero if denominator is somehow zero (though data_range check should prevent)
        if (Math.Abs(denominator) < Constants.Epsilon) return -1000; // Large penalty.

        return 1.0 - 0.5 * (term1 + term2) / denominator;
    }

    // --- Prunning versions ---

    private static double SimplicityMax(double q, int j)
    {
        int i = -1;
        for (int idx = 0; idx < Q.Length; idx++) {
            if (Math.Abs(Q[idx] - q) < Constants.Epsilon)
            { 
                i = idx + 1; 
                break; 
            } 
        }
        if (i == -1)
            throw new ArgumentException($"Base nice number q={q} not found in Q.", nameof(q));

        int v_max = 1;
        double penalty_i = (Q.Length > 1) ? (i - 1.0) / (Q.Length - 1.0) : 0;
        return 1.0 - penalty_i - j + v_max;
    }

    private double DensityMax(int i)
    {
        throw new NotImplementedException("DensityMax not implemented.");
    }

    private static double CoverageMax(double d_min, double d_max, int num_intervals, double l_step)
    {
        double data_range = d_max - d_min;
        if (Math.Abs(data_range) < Constants.Epsilon || l_step < Constants.Epsilon) return 0;

        double label_range = num_intervals * l_step;

        // Calculate distance from data extremes to ideal centered label extremes
        double dist = Math.Abs(label_range - data_range) / 2.0;
        double term1 = Math.Pow(dist, 2);

        double denominator = Math.Pow(0.1 * data_range, 2);
        if (Math.Abs(denominator) < Constants.Epsilon) return -1000;

        // Score: 1 - 0.5 * (dist^2 + dist^2) / denom = 1 - dist^2 / denom
        return 1.0 - term1 / denominator; // Since term1=term2=dist^2
    }

    // ------------------------

    private (double, double) OptLegibility(double l_min, double l_max, double l_step)
    {
        throw new NotImplementedException("OptLegibility not implemented.");
    }

    private string Label(double l_min, double l_max, double l_step, string l_format)
    {
        throw new NotImplementedException("Label not implemented.");
    }

}

public class TickLabelingResult
{

}