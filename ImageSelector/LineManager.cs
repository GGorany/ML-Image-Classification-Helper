using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageSelector
{
    internal class LineManager
    {
        public event EventHandler LineChanged;

        private readonly Line _line;
        private readonly Line _dashedLine;
        private readonly Canvas _canvas;
        private bool _isDrawing;

        private Point _mouseStartPoint;

        public Point StartPoint 
        { 
            get => new Point(_line.X1, _line.Y1);
            set => UpdateLine(value.X, value.Y, EndPoint.X, EndPoint.Y);
        }

        public Point EndPoint
        {
            get => new Point(_line.X2, _line.Y2);
            set => UpdateLine(StartPoint.X, StartPoint.Y, value.X, value.Y);
        }

        public LineManager(Canvas canvasOverlay)
        {
            _canvas = canvasOverlay;
            // intit crop rectangle
            _line = new Line()
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = 0,
                Stroke = Brushes.Red,
                StrokeThickness = 1
            };
            //intit second rectangle to fake dashed lines
            _dashedLine = new Line()
            {
                Stroke = Brushes.White,
                StrokeDashArray = new DoubleCollection(new double[] { 4, 4 })
            };
            //add both rectangels, so it will be rendered
            _canvas.Children.Add(_line);
            _canvas.Children.Add(_dashedLine);
            //set intit position on canvas
            Canvas.SetLeft(_line, 0);
            Canvas.SetTop(_line, 0);

            _line.SizeChanged += (sender, args) =>
            {
                LineChanged?.Invoke(sender, args);
            };
        }

        /// <summary>
        /// Event handler for mouse left button
        /// </summary>
        /// <param name="e">Mouse event args</param>
        public void MouseLeftButtonDownEventHandler(MouseButtonEventArgs e)
        {
            _canvas.CaptureMouse();
            Point mousePoint = e.GetPosition(_canvas);

            _mouseStartPoint = mousePoint;
            _isDrawing = true;
        }

        /// <summary>
        /// Event handler for mouse move
        /// </summary>
        /// <param name="e">Mouse event args</param>
        public void MouseMoveEventHandler(MouseEventArgs e)
        {
            //get mouse click point relative to canvas overlay
            Point mousePoint = e.GetPosition(_canvas);

            if (_isDrawing)
            {
                double x1 = _mouseStartPoint.X;
                double y1 = _mouseStartPoint.Y;
                double x2 = mousePoint.X;
                double y2 = mousePoint.Y;

                // limit
                if (x1 < 0) x1 = 0;
                if (y1 < 0) y1 = 0;
                if (x2 < 0) x2 = 0;
                if (y2 < 0) y2 = 0;
                if (x1 > _canvas.ActualWidth) x1 = _canvas.ActualWidth;
                if (y1 > _canvas.ActualHeight) y1 = _canvas.ActualHeight;
                if (x2 > _canvas.ActualWidth) x2 = _canvas.ActualWidth;
                if (y2 > _canvas.ActualHeight) y2 = _canvas.ActualHeight;

                UpdateLine(x1, y1, x2, y2);
                return;
            }
        }

        /// <summary>
        /// Event handler for mouse left button up
        /// </summary>
        /// <param name="e">Mouse event args</param>
        public void MouseLeftButtonUpEventHandler()
        {
            _isDrawing = false;
            _canvas.ReleaseMouseCapture();
        }

        public void UpdateLine(double x1, double y1, double x2, double y2)
        {
            //dont use negative value
            if (x1 >= 0 && y1 >= 0 && x2 >= 0 && y2 >= 0)
            {
                _line.X1 = x1;
                _line.Y1 = y1;
                _line.X2 = x2;
                _line.Y2 = y2;
                //we need to update dashed rectangle too
                UpdateDashedLine();
            }
        }

        private void UpdateDashedLine()
        {
            _dashedLine.X1 = _line.X1;
            _dashedLine.Y1 = _line.Y1;
            _dashedLine.X2 = _line.X2;
            _dashedLine.Y2 = _line.Y2;
        }
    }
}
