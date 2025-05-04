# SharpSvgPlotter

A .NET library for creating basic SVG plots (line, scatter and histograms) directly from your C# applications.

## Features

* **SVG Output:** Generates plot images in the Scalable Vector Graphics (SVG) format.
* **Plot Types:**
    * Line plots (`LineSeries`)
    * Scatter plots (`ScatterSeries`)
    * Histograms (`HistogramSeries`)
* **Customization:**
    * **General Plot:** Width, height, margins, background color.
    * **Title:** Add a plot title with customizable text, font size, color, and family. Hide title by leaving the text empty.
    * **Plot Area:** Customize the border color, width, and fill color of the main plotting area.
    * **Axes:**
        * Set axis labels.
        * Enable/disable auto-scaling based on data.
        * Customize axis line color/width, title font/size/color, tick mark length/color/width, and tick label font/size/color.
        * Choose from different tick generation algorithms (`HeckBert`, `GnuPlot`, `Matplotlib`).
        * Specify number format strings for tick labels (e.g., "F2", "E1", "G3").
    * **Data Series Styling:**
        * **Line:** Control stroke color, width, and dash pattern (`StrokeDashArray`).
        * **Scatter:** Control marker shape (`Circle`, `Square`), size, fill color, stroke color, and stroke width. Control opacity.
    * **Legend:**
        * Optionally display a legend for plotted series.
        * Position the legend inside the plot area (TopLeft, TopRight, BottomLeft, BottomRight).
        * Customize legend font, colors, padding, background color/opacity, and border.
* **Simple API:** Designed for straightforward integration into .NET projects.

## Building the Project

### Prerequisites

* [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)

### Using .NET CLI

1.  Clone the repository:
    ```bash
    git clone <your-repository-url>
    cd SharpSvgPlotter
    ```
2.  Navigate to the project directory.
3.  Build the project:
    ```bash
    dotnet build
    ```
    (Use `dotnet build -c Release` for a release build).

### Using Visual Studio

1.  Clone the repository.
2.  Open the solution file (`.sln`) or the project file (`SharpSvgPlotter.csproj`) in Visual Studio (2022 or later recommended).
3.  Select a build configuration (e.g., Debug, Release).
4.  Go to the `Build` menu and select `Build Solution` (or press `F6` or `Ctrl+Shift+B`).

The compiled library (`SharpSvgPlotter.dll`) will typically be found in the `bin/[Configuration]/[TargetFramework]` subdirectory (e.g., `bin/Debug/net9.0`).

## Getting Started

## Getting Started & Examples

To see how to use SharpSvgPlotter, please refer to the example project located in the following directory:

[**Link to Demo Project**](https://github.com/Cemonix/SharpSvgPlotter/tree/main/SharpSvgPlotter.Demo)

The demo project showcases how to:
* Instantiate and configure `PlotOptions`.
* Create `Plot` objects.
* Set up axes and legends.
* Define data and styles for different series types.
* Save plots to SVG files.

## Customization Overview

You can customize various aspects by modifying the `PlotOptions` object before creating the `Plot`. This object contains nested option classes (`TitleOptions`, `AxesOptions`, `LegendOptions`, etc.) for logical grouping.

```csharp
// Create default options
var options = new SharpSvgPlotter.PlotOptions.PlotOptions();

// --- Access nested options to customize ---

// Example: Change title and hide legend
options.TitleOptions.Title = "Specific Analysis";
options.TitleOptions.FontSize = 20;
options.LegendOptions.ShowLegend = false;

// Example: Customize X-axis ticks and legend position
options.XAxisOptions.TickFormatString = "N2"; // Number format with 2 decimal places
options.XAxisOptions.TickLabelColor = "blue";
options.LegendOptions.ShowLegend = true;
options.LegendOptions.Location = SharpSvgPlotter.PlotOptions.LegendLocation.BottomRight;
options.LegendOptions.BackgroundColor = "lightyellow";

// --- Create Plot with customized options ---
var customPlot = new Plot(options);

// --- Configure Axes and Add Series ---
customPlot.SetXAxis("X Data");
customPlot.SetYAxis("Y Data");
// ... Add Series ...

// --- Save ---
customPlot.Save("custom_plot.svg");

// See the demo project for more detailed examples!
```

## Future Plans
- Bar plots
- More legend positioning options (e.g., "Best")
- More marker types for scatter plots
- Improved text measurement for precise layout

## License

This project is licensed under the GNU General Public License v3.0.

See the LICENSE file for details, or visit https://www.gnu.org/licenses/gpl-3.0.html.
