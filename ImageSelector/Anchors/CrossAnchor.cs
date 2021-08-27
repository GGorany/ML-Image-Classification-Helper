using System.Windows;
using System.Windows.Media;

namespace ImageSelector.Anchors
{
    public class CrossAnchor : Anchor
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Point center = new Point(base.Position.X, base.Position.Y);
            Point top = new Point(center.X, center.Y - 5);
            Point bottom = new Point(center.X, center.Y + 5);
            Point left = new Point(center.X - 5, center.Y);
            Point right = new Point(center.X + 5, center.Y);
            Pen pen = new Pen(Brushes.Red, 1.0);

            //10x10 cross
            drawingContext.DrawLine(pen, top, bottom);
            drawingContext.DrawLine(pen, left, right);
        }
    }
}
