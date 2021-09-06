using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageSelector
{
    /// <summary>
    /// Selector.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Rectangler : UserControl
    {
        private RectangleAdorner RectangleAdorner;
        private double magnification = 1.0;
        private bool isSetFromSource = false;
        private bool isSquareMode = false;

        #region DependencyProperties
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(ImageSource),
            typeof(Rectangler),
            new PropertyMetadata(default(ImageSource), OnSourceChanged));

        public static readonly DependencyProperty RectProperty = DependencyProperty.Register(
            "Rect",
            typeof(Rect),
            typeof(Rectangler),
            new PropertyMetadata(default(Rect), OnRectChanged));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public Rect Rect
        {
            get { return (Rect)GetValue(RectProperty); }
            set { SetValue(RectProperty, value); }
        }
        #endregion

        #region Constructor
        public Rectangler()
        {
            InitializeComponent();

            _SourceImage.LayoutTransform = new ScaleTransform();
            _Canvas.LayoutTransform = new ScaleTransform();

            _MouseHandler.MouseWheel += _MouseHandler_MouseWheel;

            _Canvas.Loaded += _Canvas_Loaded;
            _Canvas.MouseLeftButtonDown += _Canvas_MouseLeftButtonDown;

            _ModeToggle.Click += _ModeToggle_Click; ;

            _Size.Text = $" Size: 0 x 0";
            _Zoom.Text = $" Zoom: {magnification * 100:N0}%";
        }
        #endregion

        #region Events - Properties
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Rectangler rectangler))
                return;

            if (e.NewValue is ImageSource newImage)
            {
                rectangler._SourceImage.Source = newImage;
                rectangler._Size.Text = $" Size: {(int)newImage.Width} x {(int)newImage.Height}";

                if (rectangler.Rect.Contains(newImage.Width, newImage.Height))
                    rectangler.Rect = new Rect(0, 0, newImage.Width, newImage.Height);

                rectangler.AdornerRect(rectangler.Rect);
            }
            else
            {
                rectangler._SourceImage.Source = null;
                rectangler.RectangleAdorner.Rect = Rect.Empty;
            }
        }

        private static void OnRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Rectangler rectangler))
                return;

            if (rectangler.RectangleAdorner == default)
                return;

            if (rectangler.isSetFromSource)
            {
                rectangler.isSetFromSource = false;
                return;
            }

            if (e.NewValue != e.OldValue)
            {
                rectangler.AdornerRect((Rect)e.NewValue);
            }
            else
            {
                rectangler.RectangleAdorner.Rect = Rect.Empty;
            }

            rectangler.UpdateRegionText();
        }
        #endregion

        #region Events
        private void _MouseHandler_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point point = Mouse.GetPosition(_Canvas);

            double zoom_delta = e.Delta > 0 ? .1 : -.1;
            magnification = (magnification += zoom_delta).LimitToRange(.1, 10);
            ApplyMagnification();

            // Center Viewer Around Mouse Position
            if (_ScrollViewer != null)
            {
                _ScrollViewer.ScrollToHorizontalOffset(point.X * magnification - Mouse.GetPosition(_ScrollViewer).X);
                _ScrollViewer.ScrollToVerticalOffset(point.Y * magnification - Mouse.GetPosition(_ScrollViewer).Y);
            }

            e.Handled = true;
        }

        private void _Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement visual)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(visual);
                if (adornerLayer == null)
                    return;

                RectangleAdorner = new RectangleAdorner(visual);
                adornerLayer.Add(RectangleAdorner);
                AdornerRect(Rect);
                RectangleAdorner.OnRectangleSizeEvent += SelectingAdorner_OnRectangleSizeEvent;
            }
        }

        private void _Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RectangleAdorner.MouseLeftButtonDownEventHandler(sender, e);
        }

        private void SelectingAdorner_OnRectangleSizeEvent(object sender, Rect rect)
        {
            if (_SourceImage?.Source == null)
                return;

            if (RectangleAdorner == null)
                return;

            if (Rect == rect)
                return;

            isSetFromSource = true;
            Rect = rect;
            UpdateRegionText();
        }

        private void _ModeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_ModeToggle.IsChecked == true)
                isSquareMode = true;
            else
                isSquareMode = false;

            RectangleAdorner.IsSquareMode = isSquareMode;
        }
        #endregion

        #region Private Method
        private void ApplyMagnification()
        {
            if (_SourceImage != null)
            {
                ScaleTransform obj = (ScaleTransform)_SourceImage.LayoutTransform;
                obj.ScaleX = obj.ScaleY = magnification;
                RenderOptions.SetBitmapScalingMode(_SourceImage, BitmapScalingMode.HighQuality);
                _Zoom.Text = $" Zoom: {magnification * 100:N0}%";
            }

            if (_Canvas != null)
            {
                ScaleTransform obj2 = (ScaleTransform)_Canvas.LayoutTransform;
                obj2.ScaleX = obj2.ScaleY = magnification;
            }
        }

        private void UpdateRegionText()
        {
            _Region.Text = $" X: {Rect.X} Y: {Rect.Y} ({Rect.Width} x {Rect.Height})";
        }

        private void AdornerRect(Rect rect)
        {
            if (RectangleAdorner == null) 
                return;

            if (_SourceImage.Source == null) 
                return;

            RectangleAdorner.Rect = rect;
        }
        #endregion
    }
}
