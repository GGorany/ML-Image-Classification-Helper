using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ImageSelector
{
    internal class ThumbCircle : Thumb, IThumb
    {
        public double ThumbSize { get; private set; }

        public ThumbCircle(double thumbSize)
        {
            ThumbSize = thumbSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(Brushes.White, new Pen(Brushes.Red, 2), new Point(ThumbSize / 2, ThumbSize / 2), 4, 4);
            drawingContext.DrawEllipse(Brushes.Red, new Pen(Brushes.Red, 0), new Point(ThumbSize / 2, ThumbSize / 2), 2, 2);
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
