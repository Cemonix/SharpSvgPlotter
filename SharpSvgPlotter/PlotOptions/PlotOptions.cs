using System;
using System.Globalization;
using SharpSvgPlotter.AxisLabeling;
using SharpSvgPlotter.Primitives;

namespace SharpSvgPlotter.PlotOptions;

public class PlotOptions
{
    // --- General ---
    public double Width { get; set; } = 800;
    public double Height { get; set; } = 600;
    public PlotMargins Margins { get; set; } = new (50, 50, 50, 50);
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public AxisLabelingAlgorithmType? LabelingAlgorithm { get; set; }
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture; // For number formatting

    // --- Plot Title ---
    public TitleOptions TitleOptions { get; set; } = new TitleOptions();

    // --- Plot Area ---
    public PlotAreaOptions PlotAreaOptions { get; set; } = new PlotAreaOptions();

    // --- Axes ---
    public AxisOptions XAxisOptions { get; set; } = new AxisOptions();
    public AxisOptions YAxisOptions { get; set; } = new AxisOptions();

    // --- Legend ---
    public LegendOptions LegendOptions { get; set; } = new LegendOptions();
}
