using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ImageSelector
{
    internal class ThumbLineManager : IThumbManager
    {
        private readonly ThumbCircle _startPoint, _endPoint;
        private readonly Canvas _canvas;
        private readonly LineManager _lineManager;
        private readonly double _thumbSize;

        public ThumbLineManager(Canvas canvas, LineManager lineManager)
        {
            //  initizalize
            _canvas = canvas;
            _lineManager = lineManager;
            _thumbSize = 10;

            //  create thumbs with factory
            _startPoint = ThumbFactory.CreateThumbCircle(_canvas, _thumbSize);
            _endPoint = ThumbFactory.CreateThumbCircle(_canvas, _thumbSize);

            //  subsctibe to mouse events
            _startPoint.DragDelta += new DragDeltaEventHandler(StartPointDragDeltaEventHandler);
            _startPoint.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PreviewMouseLeftButtonDownGenericHandler);
            _startPoint.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(PreviewMouseLeftButtonUpGenericHandler);

            _endPoint.DragDelta += new DragDeltaEventHandler(EndPointDragDeltaEventHandler);
            _endPoint.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PreviewMouseLeftButtonDownGenericHandler);
            _endPoint.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(PreviewMouseLeftButtonUpGenericHandler);
        }

        private void StartPointDragDeltaEventHandler(object sender, DragDeltaEventArgs e)
        {
            ThumbCircle thumb = sender as ThumbCircle;

            double newX = Canvas.GetLeft(thumb) + e.HorizontalChange;
            double newY = Canvas.GetTop(thumb) + e.VerticalChange;

            if (newX < 0)
                newX = 0;

            if (newY < 0)
                newY = 0;

            if (newX > _canvas.ActualWidth)
                newX = _canvas.ActualWidth;

            if (newY > _canvas.ActualHeight)
                newY = _canvas.ActualHeight;

            UpdateLine(newX, newY, _lineManager.EndPoint.X, _lineManager.EndPoint.Y);
        }

        private void EndPointDragDeltaEventHandler(object sender, DragDeltaEventArgs e)
        {
            ThumbCircle thumb = sender as ThumbCircle;

            double newX = Canvas.GetLeft(thumb) + e.HorizontalChange;
            double newY = Canvas.GetTop(thumb) + e.VerticalChange;

            if (newX < 0)
                newX = 0;

            if (newY < 0)
                newY = 0;

            if (newX > _canvas.ActualWidth)
                newX = _canvas.ActualWidth;

            if (newY > _canvas.ActualHeight)
                newY = _canvas.ActualHeight;

            UpdateLine(_lineManager.StartPoint.X, _lineManager.StartPoint.Y, newX, newY);
        }

        /// <summary>
        /// Update (redraw) thumbs positions
        /// </summary>
        public void UpdateThumbsPosition()
        {
            if (_lineManager.StartPoint != _lineManager.EndPoint)
            {
                _startPoint.SetPosition(_lineManager.StartPoint.X, _lineManager.StartPoint.Y);
                _endPoint.SetPosition(_lineManager.EndPoint.X, _lineManager.EndPoint.Y);
            }
        }

        /// <summary>
        /// Manage thumbs visibility
        /// </summary>
        /// <param name="isVisble">Set current visibility</param>
        public void ShowThumbs(bool isVisble)
        {
            if (isVisble && (_lineManager.StartPoint != _lineManager.EndPoint))
            {
                _startPoint.Visibility = Visibility.Visible;
                _endPoint.Visibility = Visibility.Visible;
            }
            else
            {
                _startPoint.Visibility = Visibility.Hidden;
                _endPoint.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateLine(double x1, double y1, double x2, double y2)
        {
            _lineManager.UpdateLine(x1, y1, x2, y2);
            UpdateThumbsPosition();
        }

        private void PreviewMouseLeftButtonDownGenericHandler(object sender, MouseButtonEventArgs e)
        {
            ThumbCircle thumb = sender as ThumbCircle;
            thumb.CaptureMouse();
        }

        private void PreviewMouseLeftButtonUpGenericHandler(object sender, MouseButtonEventArgs e)
        {
            ThumbCircle thumb = sender as ThumbCircle;
            thumb.ReleaseMouseCapture();
        }
    }
}
