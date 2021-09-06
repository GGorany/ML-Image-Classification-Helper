using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageSelector
{
    internal class RectangleAdorner : Adorner
    {
        public event EventHandler<Rect> OnRectangleSizeEvent;

        private readonly RectangleManager _rectangleManager;
        private readonly IOverlayManager _overlayManager;
        private readonly IThumbManager _thumbManager;

        private bool _isMouseLeftButtonDown;
        private readonly VisualCollection _visualCollection;
        private readonly Canvas _canvasOverlay;
        private readonly Canvas _originalCanvas;

        public RectangleAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _visualCollection = new VisualCollection(this);
            _originalCanvas = (Canvas)adornedElement;
            _canvasOverlay = new Canvas();
            _rectangleManager = new RectangleManager(_canvasOverlay);
            _overlayManager = new OverlayRectManager(_canvasOverlay, _rectangleManager);
            _thumbManager = new ThumbRectManager(_canvasOverlay, _rectangleManager);
            _visualCollection.Add(_canvasOverlay);

            //add event handlers
            MouseLeftButtonDown += MouseLeftButtonDownEventHandler;
            MouseMove += MouseMoveEventHandler;
            MouseLeftButtonUp += MouseLeftButtonUpEventHandler;
            Loaded += (object sender, RoutedEventArgs args) => Show();
            _originalCanvas.SizeChanged += (object sender, SizeChangedEventArgs e) => Show();
            _rectangleManager.RectangleSizeChanged += (object sender, EventArgs args) => Show();
        }

        public bool IsSquareMode
        {
            get => _rectangleManager.IsSquareMode;
            set => _rectangleManager.IsSquareMode = value;
        }

        public Rect Rect
        {
            get => _rectangleManager.Rect;
            set
            {
                _rectangleManager.Rect = value;
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
            _rectangleManager.MouseLeftButtonDownEventHandler(e);
            _overlayManager.UpdateOverlay();
            if (_rectangleManager.RectangleWidth == 0 && _rectangleManager.RectangleHeight == 0)
            {
                _thumbManager.ShowThumbs(false);
            }
            _isMouseLeftButtonDown = true;
        }

        public void Show()
        {
            if (Rect.IsEmpty)
            {
                _overlayManager.UpdateOverlay();
                _thumbManager.ShowThumbs(false);
                _thumbManager.UpdateThumbsPosition();
            }
            else
            {
                _overlayManager.UpdateOverlay();
                _thumbManager.ShowThumbs(true);
                _thumbManager.UpdateThumbsPosition();
            }
        }

        private void MouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            if (_isMouseLeftButtonDown)
            {
                _rectangleManager.MouseMoveEventHandler(e);
                Show();
            }

            OnRectangleSizeEvent?.Invoke(sender, _rectangleManager.Rect);
        }

        private void MouseLeftButtonUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            _rectangleManager.MouseLeftButtonUpEventHandler();
            ReleaseMouseCapture();
            _isMouseLeftButtonDown = false;
            OnRectangleSizeEvent?.Invoke(sender, _rectangleManager.Rect);
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
