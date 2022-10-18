using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SiszarpOnTheDisco.Plugins;

public class LiveChartsGenerator
{
    public async Task<string> GetPowerChartPath(DateTime[] x, double[] y)
    {
        SKColor[] colors =
        {
            SKColors.Orange,
            SKColors.Transparent
        };

        SKCartesianChart line = new SKCartesianChart
        {
            Width = 900,
            Height = 600,
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = y,
                    GeometrySize = 0,
                    // Stroke = new LinearGradientPaint(colors, new SKPoint(0,0), new SKPoint(0,1.5f))  { StrokeThickness = 1 },
                    Stroke = new SolidColorPaint(SKColors.Firebrick) { StrokeThickness = 1 },
                    //GeometryStroke = new LinearGradientPaint(colors) { StrokeThickness = 10 },
                    // Fill = new LinearGradientPaint(colors) { StrokeThickness = 10 },
                    Fill = new LinearGradientPaint(colors, new SKPoint(0, 0), new SKPoint(0, 1.5f))
                        { StrokeThickness = 10, Style = SKPaintStyle.Stroke },
                    GeometryFill = new LinearGradientPaint(colors) { StrokeThickness = 10 }
                }
            },

            XAxes = new List<Axis>
            {
                new()
                {
                    Labeler = value => new DateTime((long)value).ToString("HH:mm"),

                    Labels = Array.ConvertAll(x, o => { return o.ToString("HH:mm"); }).ToList(),
                    // Labels = Array.ConvertAll(x, o => { return  Convert.ToString(TimeSpan.from(o.Hour)); }).ToList(),


                    LabelsRotation = 90,
                    IsVisible = true,


                    MinStep = TimeSpan.FromMilliseconds(1).Ticks,
                    UnitWidth = TimeSpan.FromHours(1).Ticks,

                    //ForceStepToMin = true,

                    ShowSeparatorLines = true
                }
            },

            YAxes = new List<Axis>
            {
                new()
                {
                    IsVisible = true,
                    UnitWidth = 10,
                    ShowSeparatorLines = true,
                    SeparatorsPaint = new SolidColorPaint
                    {
                        Color = SKColors.Gray, StrokeThickness = 1,
                        PathEffect = new DashEffect(new float[] { 10, 3 })
                    },
                    Name = "kW",
                    MinLimit = 0,
                    MaxLimit = y.Max() + 1
                }
            },

            DrawMarginFrame = new DrawMarginFrame
            {
                Fill = new SolidColorPaint
                {
                    Color = new SKColor(0, 0, 0, 30)
                },
                Stroke = new SolidColorPaint
                {
                    Color = new SKColor(80, 80, 80),
                    StrokeThickness = 2
                }
            }
        };

        string path = CreatePictureLocalPath();

        line.SaveImage(path);
        return path;
    }

    private string CreatePictureLocalPath()
    {
        DirectoryInfo directoryInfo = new("/pics");

        if (!directoryInfo.Exists) directoryInfo.Create();

        long timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        return Path.Combine(directoryInfo.FullName, $"{timeStamp}.png");
    }
}