namespace SharpSvgPlotter.Series.HistogramSupport;

public enum AutomaticBinningRule
{
    /// <summary>
    /// Uses sqrt(N) for number of bins. Simple default.
    /// </summary>
    SquareRoot,
    /// <summary>
    /// Uses log2(N)+1 for number of bins. Assumes normality.
    /// </summary>
    Sturges,
    /// <summary>
    /// Uses standard deviation to determine bin width. Assumes normality.
    /// </summary>
    Scott,
    /// <summary>
    /// Uses Interquartile Range (IQR) to determine bin width. Robust to outliers.
    /// </summary>
    FreedmanDiaconis
}
