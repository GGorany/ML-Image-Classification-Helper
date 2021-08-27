using ImageSelector.Anchors;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageSelector.ROIs
{
    public class ROIRect : ROI
    {
        #region DependencyProperties
        public static readonly DependencyProperty TopLeftProperty = DependencyProperty.Register(
            "TopLeft",
            typeof(Point),
            typeof(ROIRect),
            new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender, OnTopLeftChanged));

        public static readonly DependencyProperty BottomRightProperty = DependencyProperty.Register(
            "BottomRight",
            typeof(Point),
            typeof(ROIRect),
            new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender, OnBottomRightChanged));

        public Point TopLeftPoint
        {
            get { return (Point)GetValue(TopLeftProperty); }
            set { SetValue(TopLeftProperty, value); }
        }

        public Point BottomRightPoint
        {
            get { return (Point)GetValue(BottomRightProperty); }
            set { SetValue(BottomRightProperty, value); }
        }
        #endregion

        //Anchors
        const int TOP_LEFT = 0, TOP_RIGHT = 1, BOTTOM_LEFT = 2, BOTTOM_RIGHT = 3, CENTER = 4;

        public ROIRect()
        {
            //four anchor points and one center point
            for (int i = 0; i < 4; i++) 
                base.Anchors.Add(AnchorsFactory.Create(AnchorType.Resize, this));

            base.Anchors.Add(AnchorsFactory.Create(AnchorType.Move, this));
            base.MouseLeftButtonDown += OnRectROIMouseLeftButtonDown;
            base.MouseLeftButtonUp += OnRectROIMouseLeftButtonUp;
            base.MouseMove += OnRectROIMouseMove;
        }

        private void OnRectROIMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (base.CurrentState == State.Normal)
            {
                actPos = prevPos = e.GetPosition(this);
                base.CurrentState = State.Selected;
            }
            CaptureMouse();
            actElt = e.OriginalSource;
        }

        private void OnRectROIMouseMove(object sender, MouseEventArgs e)
        {
            actPos = e.GetPosition(this);
            Point diff = new Point(prevPos.X - actPos.X, prevPos.Y - actPos.Y);
            prevPos = actPos;

            switch (base.CurrentState)
            {
                case State.DrawingInProgress:
                    BottomRightPoint = e.GetPosition(this);
                    break;
                case State.Selected:
                    if (actElt == base.Anchors[TOP_LEFT])
                    { //trivial case
                        TopLeftPoint = new Point(TopLeftPoint.X - diff.X, TopLeftPoint.Y - diff.Y);
                    }
                    else if (actElt == base.Anchors[BOTTOM_RIGHT])
                    { //trivial case
                        BottomRightPoint = new Point(BottomRightPoint.X - diff.X, BottomRightPoint.Y - diff.Y);
                    }
                    else if (actElt == base.Anchors[TOP_RIGHT])
                    {
                        Point newAnchor = new Point(base.Anchors[TOP_RIGHT].Position.X - diff.X, base.Anchors[TOP_RIGHT].Position.Y - diff.Y);
                        TopLeftPoint = new Point(TopLeftPoint.X, newAnchor.Y);
                        BottomRightPoint = new Point(newAnchor.X, BottomRightPoint.Y);
                    }
                    else if (actElt == base.Anchors[BOTTOM_LEFT])
                    {
                        Point newAnchor = new Point(base.Anchors[BOTTOM_LEFT].Position.X - diff.X, base.Anchors[BOTTOM_LEFT].Position.Y - diff.Y);
                        TopLeftPoint = new Point(newAnchor.X, TopLeftPoint.Y);
                        BottomRightPoint = new Point(BottomRightPoint.X, newAnchor.Y);
                    }
                    else if (actElt == this || actElt == base.Anchors[CENTER])
                    {
                        TopLeftPoint = new Point(TopLeftPoint.X - diff.X, TopLeftPoint.Y - diff.Y);
                        BottomRightPoint = new Point(BottomRightPoint.X - diff.X, BottomRightPoint.Y - diff.Y);
                    }
                    break;
            }
            UpdateLastROIDrawEvent(new ROIDescriptor.LastEventArgs(GetLastDrawEventData()));
        }

        private void OnRectROIMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            base.CurrentState = State.Normal;
        }

        private static void OnTopLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROIRect obj = (ROIRect)d;
            Point topLeft = (Point)e.NewValue;
            Point topRight = obj.Anchors[TOP_RIGHT].Position;
            topRight.Y = topLeft.Y;
            Point bottomLeft = obj.Anchors[BOTTOM_LEFT].Position;
            bottomLeft.X = topLeft.X;
            Point center = obj.Anchors[CENTER].Position;
            center.X = (topLeft.X + topRight.X) / 2.0;
            center.Y = (topLeft.Y + bottomLeft.Y) / 2.0;
            obj.Anchors[TOP_LEFT].Position = topLeft;
            obj.Anchors[TOP_RIGHT].Position = topRight;
            obj.Anchors[BOTTOM_LEFT].Position = bottomLeft;
            obj.Anchors[CENTER].Position = center;
        }

        private static void OnBottomRightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROIRect obj = (ROIRect)d;
            Point bottomRight = (Point)e.NewValue;
            Point topRight = obj.Anchors[TOP_RIGHT].Position;
            topRight.X = bottomRight.X;
            Point bottomLeft = obj.Anchors[BOTTOM_LEFT].Position;
            bottomLeft.Y = bottomRight.Y;
            Point center = obj.Anchors[CENTER].Position;
            center.X = (bottomRight.X + bottomLeft.X) / 2.0;
            center.Y = (bottomRight.Y + topRight.Y) / 2.0;
            obj.Anchors[TOP_RIGHT].Position = topRight;
            obj.Anchors[BOTTOM_LEFT].Position = bottomLeft;
            obj.Anchors[BOTTOM_RIGHT].Position = bottomRight;
            obj.Anchors[CENTER].Position = center;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            //Pen pen = new Pen(Brushes.Red, 2.0 / base.Magnification);
            Pen pen = new Pen(Brushes.Red, 1.0);
            Rect rect;

            rect.X = Math.Min(TopLeftPoint.X, BottomRightPoint.X);
            rect.Y = Math.Min(TopLeftPoint.Y, BottomRightPoint.Y);
            rect.Width = Math.Abs(BottomRightPoint.X - TopLeftPoint.X);
            rect.Height = Math.Abs(BottomRightPoint.Y - TopLeftPoint.Y);

            dc.DrawRectangle(Brushes.Transparent, pen, rect);
        }

        public override ROIDescriptor.LastEventData GetLastDrawEventData()
        {
            double width = Math.Abs(BottomRightPoint.X - TopLeftPoint.X);
            double height = Math.Abs(BottomRightPoint.Y - TopLeftPoint.Y);
            double diagonal = Math.Sqrt(width * width + height * height);
            return new ROIDescriptor.LastEventData
            {
                type = EventType.Draw,
                tool = EventTool.ROI,
                roi = ROItype.Rectangle,
                coordinates = new List<Point>{
                    TopLeftPoint,
                    BottomRightPoint
                },
                otherParameters = new List<double>{
                    width,
                    height,
                    diagonal,
                }
            };
        }

        public override ROIDescriptor.Contour GetROIDescriptorContour()
        {
            return new ROIDescriptor.Contour
            {
                roiType = ROItype.Rectangle,
                points = new List<Point>{
                    TopLeftPoint,
                    BottomRightPoint
                }
            };
        }
    }
}
