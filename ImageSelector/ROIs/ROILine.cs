using ImageSelector.Anchors;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageSelector.ROIs
{
    public class ROILine : ROI
    {
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
            "StartPoint",
            typeof(Point),
            typeof(ROILine),
            new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender, OnStartPointChanged)); //.AD. OnChange added 21FEB2020
        
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
            "EndPoint",
            typeof(Point),
            typeof(ROILine),
            new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender, OnEndPointChanged));

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

        const int START = 0, END = 1;

        public ROILine()
        {
            base.Anchors.Add(AnchorsFactory.Create(AnchorType.Resize, this));
            base.Anchors.Add(AnchorsFactory.Create(AnchorType.Resize, this));

            base.MouseLeftButtonDown += OnLineROIMouseLeftButtonDown;
            base.MouseLeftButtonUp += OnLineROIMouseLeftButtonUp;
            base.MouseMove += OnLineROIMouseMove;
        }

        //Down Move Up:
        private void OnLineROIMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
            if (base.CurrentState == State.Normal)
            {
                base.CurrentState = State.Selected;
                actPos = prevPos = e.GetPosition(this);
            }
            actElt = e.OriginalSource;
        }

        private void OnLineROIMouseMove(object sender, MouseEventArgs e)
        {
            actPos = e.GetPosition(this);
            Point diff = new Point(prevPos.X - actPos.X, prevPos.Y - actPos.Y);
            prevPos = actPos;

            switch (base.CurrentState)
            {
                case State.DrawingInProgress:
                    EndPoint = actPos;
                    break;
                case State.Selected:
                    //ToDo: avoid collapse here!
                    if (actElt == base.Anchors[START]) 
                        StartPoint = new Point(StartPoint.X - diff.X, StartPoint.Y - diff.Y);
                    else if (actElt == base.Anchors[END]) 
                        EndPoint = new Point(EndPoint.X - diff.X, EndPoint.Y - diff.Y);
                    else if (actElt == this)
                    {
                        StartPoint = new Point(StartPoint.X - diff.X, StartPoint.Y - diff.Y);
                        EndPoint = new Point(EndPoint.X - diff.X, EndPoint.Y - diff.Y);
                    }
                    break;
            }
            UpdateLastROIDrawEvent(new ROIDescriptor.LastEventArgs(GetLastDrawEventData()));
        }

        private void OnLineROIMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            base.CurrentState = State.Normal;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            Pen pen = new Pen(Brushes.Red, 1.0);
            dc.DrawLine(pen, StartPoint, EndPoint);
        }

        private static void OnStartPointChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((ROILine)obj).Anchors[START].Position = (Point)e.NewValue;
        }

        private static void OnEndPointChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((ROILine)obj).Anchors[END].Position = (Point)e.NewValue;
        }

        public override ROIDescriptor.LastEventData GetLastDrawEventData()
        {
            double width = Math.Abs(EndPoint.X - StartPoint.X);
            double height = Math.Abs(EndPoint.Y - StartPoint.Y);
            double diagonal = Math.Sqrt(width * width + height * height);
            double angle = (Math.Atan2(0.0 - (EndPoint.Y - StartPoint.Y), EndPoint.X - StartPoint.X) * 180.0 / Math.PI);
            return new ROIDescriptor.LastEventData
            {
                type = EventType.Draw,
                tool = EventTool.ROI,
                roi = ROItype.Line,
                coordinates = new List<Point>{
                    StartPoint,
                    EndPoint
                },
                otherParameters = new List<double>{
                    width,
                    height,
                    diagonal,
                    angle
                }
            };
        }

        public override ROIDescriptor.Contour GetROIDescriptorContour()
        {
            return new ROIDescriptor.Contour
            {
                roiType = ROItype.Line,
                points = new List<Point>{
                    StartPoint,
                    EndPoint
                }
            };
        }
    }
}
