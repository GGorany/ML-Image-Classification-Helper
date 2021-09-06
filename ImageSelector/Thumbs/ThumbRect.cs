using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ImageSelector
{
    internal class ThumbRect : Thumb, IThumb
    {
        public double ThumbSize { get; private set; }

        public ThumbRect(double thumbSize)
        {
            ThumbSize = thumbSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.Red, 2), new Rect(new Size(ThumbSize, ThumbSize)));
            drawingContext.DrawRectangle(Brushes.Red, new Pen(Brushes.Red, 0), new Rect(2, 2, 6, 6));
        }

        protected override Visual GetVisualChild(int index)
        {
            return null;
        }

        public void SetPosition(double x, double y)
        {
            Canvas.SetTop(this, y - ThumbSize / 2);
            Canvas.SetLeft(this, x - ThumbSize / 2);
        }
    }
}
