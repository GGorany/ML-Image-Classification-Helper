using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageSelector
{
    internal class LineChangeEventArgs : EventArgs
    {
        public Point SP { get; set; }
        public Point EP { get; set; }
    }

    internal class LineAdorner : Adorner
    {
        public event EventHandler<LineChangeEventArgs> OnLineChangeEvent;

        private readonly LineManager _lineManager;
        private readonly IOverlayManager _overlayManager;
        private readonly IThumbManager _thumbManager;

        private bool _isMouseLeftButtonDown;
        private readonly VisualCollection _visualCollection;
        private readonly Canvas _canvasOverlay;
        private readonly Canvas _originalCanvas;

        public LineAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _visualCollection = new VisualCollection(this);
            _originalCanvas = (Canvas)adornedElement;
            _canvasOverlay = new Canvas();
            _lineManager = new LineManager(_canvasOverlay);
            _overlayManager = new OverlayLineManager(_canvasOverlay, _lineManager);
            _thumbManager = new ThumbLineManager(_canvasOverlay, _lineManager);
            _visualCollection.Add(_canvasOverlay);

            //add event handlers
            MouseLeftButtonDown += MouseLeftButtonDownEventHandler;
            MouseMove += MouseMoveEventHandler;
            MouseLeftButtonUp += MouseLeftButtonUpEventHandler;
            Loaded += (object sender, RoutedEventArgs args) => Show();
            _originalCanvas.SizeChanged += (object sender, SizeChangedEventArgs e) => Show();
            _lineManager.LineChanged += (object sender, EventArgs args) => Show();
        }

        public Point StartPoint
        {
            get => _lineManager.StartPoint;
            set
            {
                _lineManager.StartPoint = value;
                Show();
            }
        }

        public Point EndPoint
        {
            get => _lineManager.EndPoint;
            set
            {
                _lineManager.EndPoint = value;
                Show();
            }
        }

        /// <summary>
        /// Mouse left button down event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
            if (e == null) throw new ArgumentNullException(nameof(e));
            _lineManager.MouseLeftButtonDownEventHandler(e);
            _overlayManager.UpdateOverlay();
            if (_lineManager.StartPoint == _lineManager.EndPoint)
            {
                _thumbManager.ShowThumbs(false);
            }
            _isMouseLeftButtonDown = true;
        }

        public void Show()
        {
            if (StartPoint == EndPoint)
                _thumbManager.ShowThumbs(false);
            else
                _thumbManager.ShowThumbs(true);

            _overlayManager.UpdateOverlay();
            _thumbManager.UpdateThumbsPosition();
        }

        private void MouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            if (_isMouseLeftButtonDown)
            {
                _lineManager.MouseMoveEventHandler(e);
                Show();
            }

            LineChangeEventArgs args = new LineChangeEventArgs();
            args.SP = _lineManager.StartPoint;
            args.EP = _lineManager.EndPoint;
            OnLineChangeEvent?.Invoke(sender, args);
        }

        private void MouseLeftButtonUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            _lineManager.MouseLeftButtonUpEventHandler();
            ReleaseMouseCapture();
            _isMouseLeftButtonDown = false;

            LineChangeEventArgs args = new LineChangeEventArgs();
            args.SP = _lineManager.StartPoint;
            args.EP = _lineManager.EndPoint;
            OnLineChangeEvent?.Invoke(sender, args);
        }

        // Override the VisualChildrenCount properties to interface with 
        // the adorner's visual collection.
        protected override int VisualChildrenCount
        {
            get { return _visualCollection.Count; }
        }

        // Override the GetVisualChild properties to interface with 
        // the adorner's visual collection.
        protected override Visual GetVisualChild(int index)
        {
            return _visualCollection[index];
        }

        // Positions child elements and determines a size
        protected override Size ArrangeOverride(Size size)
        {
            Size finalSize = base.ArrangeOverride(size);
            _canvasOverlay.Arrange(new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height));
            return finalSize;
        }
    }
}
