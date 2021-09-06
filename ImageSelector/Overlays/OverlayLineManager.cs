using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageSelector
{
    internal class OverlayLineManager : IOverlayManager
    {
        private readonly Canvas _canvas;
        private readonly LineManager _lineManager;

        private GeometryGroup _geometryGroup;

        public OverlayLineManager(Canvas canvas, LineManager lineManager)
        {
            _canvas = canvas;
            _lineManager = lineManager;
        }

        /// <summary>
        /// Update (redraw) overlay
        /// </summary>
        public void UpdateOverlay()
        {
            _geometryGroup = new GeometryGroup();

            //RectangleGeometry geometry1 = new RectangleGeometry(new Rect(new Size(_canvas.ActualWidth, _canvas.ActualHeight)));
            //RectangleGeometry geometry2 = new RectangleGeometry(
            //    new Rect(_rectangleManager.TopLeft.X, _rectangleManager.TopLeft.Y,
            //             _rectangleManager.RectangleWidth, _rectangleManager.RectangleHeight));

            //_geometryGroup.Children.Add(geometry1);
            //_geometryGroup.Children.Add(geometry2);
        }
    }
}
