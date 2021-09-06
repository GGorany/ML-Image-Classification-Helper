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
    public partial class Liner : UserControl
    {
        private LineAdorner LineAdorner;
        private double magnification = 1.0;

        #region DependencyProperties
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(ImageSource),
            typeof(Liner),
            new PropertyMetadata(default(ImageSource), OnSourceChanged));

        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
            "StartPoint",
            typeof(Point),
            typeof(Liner),
            new PropertyMetadata(default(Point), OnStartPointChanged));

        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
            "EndPoint",
            typeof(Point),
            typeof(Liner),
            new PropertyMetadata(default(Point), OnEndPointChanged));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public Point StartPoint
        {
            get { return (Point)GetValue(StartPointProperty); }
            set { SetValue(StartPointProperty, value); }
        }

        public Point EndPoint
        {
            get { return (Point)GetValue(EndPointProperty); }
            set { SetValue(EndPointProperty, value); }
        }
        #endregion

        #region Constructor
        public Liner()
        {
            InitializeComponent();

            _SourceImage.LayoutTransform = new ScaleTransform();
            _Canvas.LayoutTransform = new ScaleTransform();

            _MouseHandler.MouseWheel += _MouseHandler_MouseWheel;

            _Canvas.Loaded += _Canvas_Loaded;
            _Canvas.MouseLeftButtonDown += _Canvas_MouseLeftButtonDown;

            _Size.Text = $" Size: 0 x 0";
            _Zoom.Text = $" Zoom: {magnification * 100:N0}%";
        }
        #endregion

        #region Events - Properties
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Liner liner))
                return;

            if (e.NewValue is ImageSource newImage)
            {
                liner._SourceImage.Source = newImage;
                liner._Size.Text = $" Size: {(int)newImage.Width} x {(int)newImage.Height}";

                Rect rect = new Rect(0, 0, newImage.Width, newImage.Height);

                if (!rect.Contains(liner.StartPoint))
                    liner.StartPoint = new Point(0, 0);

                if (!rect.Contains(liner.EndPoint))
                    liner.EndPoint = new Point(0, 0);

                liner.AdornerLine(liner.StartPoint, liner.EndPoint);
            }
            else
            {
                liner._SourceImage.Source = null;
                liner.LineAdorner.StartPoint = new Point(0, 0);
                liner.LineAdorner.EndPoint = new Point(0, 0);
            }
        }

        private static void OnStartPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Liner liner))
                return;

            if (liner.LineAdorner == default)
                return;

            if (e.NewValue != e.OldValue)
            {
                liner.AdornerLine((Point)e.NewValue, liner.EndPoint);
            }
            else
            {
                liner.LineAdorner.StartPoint = new Point(0, 0);
            }

            liner.UpdateLineText();
        }

        private static void OnEndPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Liner liner))
                return;

            if (liner.LineAdorner == default)
                return;

            if (e.NewValue != e.OldValue)
            {
                liner.AdornerLine(liner.StartPoint, (Point)e.NewValue);
            }
            else
            {
                liner.LineAdorner.EndPoint = new Point(0, 0);
            }

            liner.UpdateLineText();
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

                LineAdorner = new LineAdorner(visual);
                adornerLayer.Add(LineAdorner);
                AdornerLine(StartPoint, EndPoint);
                LineAdorner.OnLineChangeEvent += LineAdorner_OnLineChangeEvent;
            }
        }

        private void _Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LineAdorner.MouseLeftButtonDownEventHandler(sender, e);
        }

        private void LineAdorner_OnLineChangeEvent(object sender, LineChangeEventArgs e)
        {
            if (_SourceImage?.Source == null)
                return;

            if (LineAdorner == null)
                return;

            if (StartPoint != e.SP)
                StartPoint = e.SP;

            if (EndPoint != e.EP)
                EndPoint = e.EP;

            UpdateLineText();
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

        private void UpdateLineText()
        {
            _Line.Text = $" SP X: {(int)StartPoint.X}, SP Y: {(int)StartPoint.Y}, EP X: {(int)EndPoint.X}, EP Y: {(int)EndPoint.Y}";
        }

        private void AdornerLine(Point sp, Point ep)
        {
            if (LineAdorner == null) 
                return;

            if (_SourceImage.Source == null) 
                return;

            LineAdorner.StartPoint = sp;
            LineAdorner.EndPoint = ep;
        }
        #endregion
    }
}
