using SharpSvgPlotter.AxisLabeling;

namespace SharpSvgPlotter.Test;

[TestClass]
public sealed class HeckBertAlgorithmTest
{
    private const double Tolerance = 1e-9;

    [TestMethod]
    public void GenerateTicks_BasicRange_ReturnsCorrectTicks()
    {
        // Arrange
        var algorithm = new HeckBertAlgorithm();
        double dataMin = 8.1;
        double dataMax = 14.1;
        double axisLength = 500; // Axis length isn't directly used by Heckbert's core calc
        var options = new AxisLabelingOptions
        {
            TickCount = 4, // Requesting 4 ticks
            FormatString = "G"    // Use "G" for general number format (like "5", "10")
        };

        var expectedPositions = new List<double> { 5.0, 10.0, 15.0 };
        var expectedLabels = new List<string> { "5", "10", "15" };
        double expectedActualMin = 5.0;
        double expectedActualMax = 15.0;

        // Act
        TickCalculationResult? result = algorithm.GenerateTicks(dataMin, dataMax, axisLength, options);

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");

        // Check actual min/max with tolerance
        Assert.AreEqual(expectedActualMin, result.ActualMin, Tolerance, "ActualMin does not match.");
        Assert.AreEqual(expectedActualMax, result.ActualMax, Tolerance, "ActualMax does not match.");

        // Check number of ticks
        Assert.AreEqual(expectedPositions.Count, result.TickPositions.Count, "Incorrect number of tick positions.");
        Assert.AreEqual(expectedLabels.Count, result.TickLabels.Count, "Incorrect number of tick labels.");

        // Check tick positions element by element with tolerance
        Assert.AreEqual(expectedPositions.Count, result.TickPositions.Count); // Redundant check, but ensures loops match
        for (int i = 0; i < expectedPositions.Count; i++)
        {
            Assert.AreEqual(expectedPositions[i], result.TickPositions[i], Tolerance, $"Tick position at index {i} does not match.");
        }

        // Check tick labels using CollectionAssert
        CollectionAssert.AreEqual(expectedLabels, result.TickLabels, "Tick labels do not match.");
    }

    [TestMethod]
    public void GenerateTicks_ZeroRange_ReturnsSingleTick()
    {
        // Arrange
        var algorithm = new HeckBertAlgorithm();
        double dataMin = 5.0;
        double dataMax = 5.0;
        double axisLength = 100;
        var options = new AxisLabelingOptions { TickCount = 5, FormatString = "G" };

        var expectedPositions = new List<double> { 5.0 };
        var expectedLabels = new List<string> { "5" };
        double expectedActualMin = 5.0;
        double expectedActualMax = 5.0;

        // Act
        TickCalculationResult? result = algorithm.GenerateTicks(dataMin, dataMax, axisLength, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedActualMin, result.ActualMin, Tolerance);
        Assert.AreEqual(expectedActualMax, result.ActualMax, Tolerance);
        Assert.AreEqual(1, result.TickPositions.Count);
        Assert.AreEqual(1, result.TickLabels.Count);
        Assert.AreEqual(expectedPositions[0], result.TickPositions[0], Tolerance);
        Assert.AreEqual(expectedLabels[0], result.TickLabels[0]);
        CollectionAssert.AreEqual(expectedPositions, result.TickPositions); // Check whole list
        CollectionAssert.AreEqual(expectedLabels, result.TickLabels);
    }

    [TestMethod]
    public void GenerateTicks_NegativeRange_ReturnsCorrectTicks()
    {
        // Arrange
        var algorithm = new HeckBertAlgorithm();
        double dataMin = -18.5;
        double dataMax = -3.2;
        double axisLength = 600;
        var options = new AxisLabelingOptions { TickCount = 6, FormatString = "G" };

        // Expected calculation: range = -3.2 - (-18.5) = 15.3
        // Nice range (round=false) = 20
        // Ideal step = 20 / (6-1) = 4
        // Nice step (round=true) = 5
        // lmin = floor(-18.5 / 5) * 5 = floor(-3.7) * 5 = -4 * 5 = -20
        // lmax = ceil(-3.2 / 5) * 5 = ceil(-0.64) * 5 = 0 * 5 = 0
        var expectedPositions = new List<double> { -20.0, -15.0, -10.0, -5.0, 0.0 };
        var expectedLabels = new List<string> { "-20", "-15", "-10", "-5", "0" };
        double expectedActualMin = -20.0;
        double expectedActualMax = 0.0;

        // Act
        TickCalculationResult? result = algorithm.GenerateTicks(dataMin, dataMax, axisLength, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedActualMin, result.ActualMin, Tolerance);
        Assert.AreEqual(expectedActualMax, result.ActualMax, Tolerance);
        CollectionAssert.AreEqual(expectedPositions, result.TickPositions);
        CollectionAssert.AreEqual(expectedLabels, result.TickLabels);
    }
}
