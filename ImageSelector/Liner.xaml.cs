using ImageSelector.ROIs;

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageSelector
{
    /// <summary>
    /// Selector.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Liner : UserControl
    {
        private Point mousePosition = new Point(0, 0);
        private ROIDescriptor _lastROIDescriptor = new ROIDescriptor();

        public event EventHandler<ROIValueChangedEventArgs> ROIValueChanged;

        #region Properties
        private double _Magnification = 1.0;

        public double Magnification
        {
            get { return _Magnification; }
            set
            {
                _Magnification = value;
                ApplyMagnification();
            }
        }
        #endregion

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

        #region DependencyProperties ROI
        private static readonly DependencyPropertyKey ROIListPropertyKey = DependencyProperty.RegisterReadOnly(
            "ROIList",
            typeof(ObservableCollection<ROI>),
            typeof(Liner),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ROIListProperty = ROIListPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey GetLastEventDataPropertyKey = DependencyProperty.RegisterReadOnly(
            "GetLastEventData",
            typeof(ROIDescriptor.LastEventData),
            typeof(Liner),
            new PropertyMetadata(null, OnGetLastEventDataChanged));

        public static readonly DependencyProperty GetLastEventDataProperty = GetLastEventDataPropertyKey.DependencyProperty;

        public ObservableCollection<ROI> ROIList
        {
            get { return (ObservableCollection<ROI>)GetValue(ROIListProperty); }
            protected set { SetValue(ROIListPropertyKey, value); }
        }

        public ROIDescriptor.LastEventData GetLastEventData
        {
            get { return (ROIDescriptor.LastEventData)GetValue(GetLastEventDataProperty); }
            protected set { SetValue(GetLastEventDataPropertyKey, value); }
        }
        #endregion

        public Liner()
        {
            InitializeComponent();

            _SourceImage.LayoutTransform = new ScaleTransform();
            _ROI.LayoutTransform = new ScaleTransform();

            _MouseHandler.MouseLeftButtonDown += _MouseHandler_MouseLeftButtonDown;
            _MouseHandler.MouseMove += _MouseHandler_MouseMove;
            _MouseHandler.MouseLeftButtonUp += _MouseHandler_MouseLeftButtonUp;
            _MouseHandler.MouseWheel += _MouseHandler_MouseWheel;

            ROIList = new ObservableCollection<ROI>();
            _ROI.ItemsSource = ROIList;
        }

        #region Events
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Liner liner)) 
                return;

            if (e.NewValue is ImageSource newImage)
            {
                liner._SourceImage.Source = newImage;
                liner._SourceImage.Width = newImage.Width;
                liner._SourceImage.Height = newImage.Height;
            }
            else
            {
                liner._SourceImage.Source = null;
                liner._SourceImage.Width = double.NaN;
                liner._SourceImage.Height = double.NaN;
            }
        }

        private static void OnStartPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Liner liner))
                return;

            if (liner.ROIList.Count != 1)
                return;

            if (e.NewValue is Point point)
                (liner.ROIList[0] as ROILine).StartPoint = point;
            else
                (liner.ROIList[0] as ROILine).StartPoint = new Point(0, 0);
        }

        private static void OnEndPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Liner liner))
                return;

            if (liner.ROIList.Count != 1)
                return;

            if (e.NewValue is Point point)
                (liner.ROIList[0] as ROILine).EndPoint = point;
            else
                (liner.ROIList[0] as ROILine).EndPoint = new Point(0, 0);
        }

        private void _MouseHandler_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == _MouseHandler)
            {
                ROIList.Clear();
                StartDrawingLineROI();
            }
        }

        private void _MouseHandler_MouseMove(object sender, MouseEventArgs e)
        {
            mousePosition = Mouse.GetPosition(_SourceImage);

            ShowMousePosition();
            ShowLinePoints();
        }

        private void _MouseHandler_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _MouseHandler.ReleaseMouseCapture();
        }

        private void _MouseHandler_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mousePosition = Mouse.GetPosition(_SourceImage);

            double zoom_delta = e.Delta > 0 ? .1 : -.1;
            Magnification = (Magnification += zoom_delta).LimitToRange(.1, 10);
            ApplyMagnification();
            ShowMousePosition();
            ShowLinePoints();

            // Center Viewer Around Mouse Position
            if (_ScrollViewer != null)
            {
                _ScrollViewer.ScrollToHorizontalOffset(mousePosition.X * Magnification - Mouse.GetPosition(_ScrollViewer).X);
                _ScrollViewer.ScrollToVerticalOffset(mousePosition.Y * Magnification - Mouse.GetPosition(_ScrollViewer).Y);
            }

            e.Handled = true;
        }

        private static void OnGetLastEventDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Liner liner = (Liner)obj;
            ROIDescriptor.LastEventData lastEventData = (ROIDescriptor.LastEventData)args.NewValue;
            ROIDescriptor.LastEventData other = (ROIDescriptor.LastEventData)args.OldValue;
            if (lastEventData.type == EventType.Draw)
            {
                ROIDescriptor lastROIDescriptor = liner._lastROIDescriptor;
                ROIDescriptor previousROIDescriptor = liner.GetROIDescriptor();
                if (!lastEventData.IsChanged(other) || !previousROIDescriptor.IsChanged(lastROIDescriptor))
                {
                    liner._lastROIDescriptor = previousROIDescriptor;
                    liner.OnROIValueChanged(new ROIValueChangedEventArgs(lastEventData, lastROIDescriptor, previousROIDescriptor));
                }
            }
        }

        private void OnGetLastDrawEventUpdated(object sender, ROIDescriptor.LastEventArgs lastEventArgs)
        {
            GetLastEventData = lastEventArgs.data;
        }

        private void OnROIValueChanged(ROIValueChangedEventArgs args)
        {
            this.ROIValueChanged?.Invoke(this, args);

            StartPoint = args.ROI.contours[0].points[0];
            EndPoint = args.ROI.contours[0].points[1];
        }
        #endregion

        #region Private Method
        private void ShowMousePosition()
        {
            Point point = Mouse.GetPosition(_SourceImage);
            if ((point.X >= 0) && (point.X < _SourceImage.Width) && (point.Y >= 0) && (point.Y < _SourceImage.Height))
            {
                _Position.Text = $"MOUSE : x = {Mouse.GetPosition(_SourceImage).X:N0}, y = {Mouse.GetPosition(_SourceImage).Y:N0}";
            }
        }

        private void ShowLinePoints()
        {
            int sp_x = 0, sp_y = 0, ep_x = 0, ep_y = 0;

            if (ROIList.Count != 0)
            {
                sp_x = (int)(ROIList[0] as ROILine).StartPoint.X;
                sp_y = (int)(ROIList[0] as ROILine).StartPoint.Y;
                ep_x = (int)(ROIList[0] as ROILine).EndPoint.X;
                ep_y = (int)(ROIList[0] as ROILine).EndPoint.Y;
            }

            _StartPoint.Text = $"START POINT : x = {sp_x:N0}, y = {sp_y:N0}";
            _EndPoint.Text = $"END POINT : x = {ep_x:N0}, y = {ep_y:N0}";
        }

        private void ApplyMagnification()
        {
            if (_SourceImage != null)
            {
                ScaleTransform obj = (ScaleTransform)_SourceImage.LayoutTransform;
                obj.ScaleX = obj.ScaleY = Magnification;
                RenderOptions.SetBitmapScalingMode(_SourceImage, BitmapScalingMode.HighQuality);
                _Zoom.Text = $"ZOOM : {Magnification * 100:N0}%";
            }

            if (_ROI != null)
            {
                ScaleTransform obj2 = (ScaleTransform)_ROI.LayoutTransform;
                obj2.ScaleX = obj2.ScaleY = Magnification;
            }
        }

        private void StartDrawingLineROI()
        {
            ROILine lineROI = new ROILine();
            ROIList.Add(lineROI);
            lineROI.EndPoint = lineROI.StartPoint = mousePosition;
            lineROI.CaptureMouse();
            lineROI.CurrentState = State.DrawingInProgress;
            lineROI.LastROIDrawEvent += OnGetLastDrawEventUpdated;
        }

        private ROIDescriptor GetROIDescriptor()
        {
            ROIDescriptor roiDescriptor = new ROIDescriptor();

            if (ROIList.Count == 0)
                return roiDescriptor;

            foreach (ROI item in ROIList)
                roiDescriptor.contours.Add(item.GetROIDescriptorContour());

            return roiDescriptor;
        }
        #endregion
    }
}
