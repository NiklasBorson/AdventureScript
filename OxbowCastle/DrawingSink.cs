using AdventureScript;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;

namespace OxbowCastle
{
    sealed class DrawingSink : IDrawingSink
    {
        static Dictionary<uint, SolidColorBrush> m_brushes = new Dictionary<uint, SolidColorBrush>();

        Canvas m_canvas;

        public DrawingSink(int width, int height)
        {
            m_canvas = new Canvas
            {
                Width = width,
                Height = height,
                Clip = new RectangleGeometry
                {
                    Rect = new Windows.Foundation.Rect(0, 0, width, height)
                }
            };
        }

        public Canvas Canvas => m_canvas;

        Brush GetBrush(uint color)
        {
            SolidColorBrush brush = null;
            if ((color & 0xFF000000) != 0 && !m_brushes.TryGetValue(color, out brush))
            {
                brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(
                    (byte)(color >> 24),
                    (byte)(color >> 16),
                    (byte)(color >> 8),
                    (byte)color
                    ));
                m_brushes.Add(color, brush);
            }
            return brush;
        }

        public void DrawRectangle(int left, int top, int width, int height, int fillColor, int strokeColor, int strokeThickness)
        {
            var rect = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Width = width,
                Height = height,
                Fill = GetBrush((uint)fillColor),
                Stroke = GetBrush((uint)strokeColor),
                StrokeThickness = strokeThickness
            };
            rect.SetValue(Canvas.LeftProperty, left);
            rect.SetValue(Canvas.TopProperty, top);
            m_canvas.Children.Add(rect);
        }

        public void DrawEllipse(int left, int top, int width, int height, int fillColor, int strokeColor, int strokeThickness)
        {
            var ellipse = new Microsoft.UI.Xaml.Shapes.Ellipse
            {
                Width = width,
                Height = height,
                Fill = GetBrush((uint)fillColor),
                Stroke = GetBrush((uint)strokeColor),
                StrokeThickness = strokeThickness
            };
            ellipse.SetValue(Canvas.LeftProperty, left);
            ellipse.SetValue(Canvas.TopProperty, top);
            m_canvas.Children.Add(ellipse);
        }
    }
}
