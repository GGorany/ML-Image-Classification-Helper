using ImageSelector.Anchors;

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ImageSelector.ROIs
{
    public abstract class ROI : ItemsControl
    {
        public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.Register(
            "CurrentState",
            typeof(State),
            typeof(ROI),
            new FrameworkPropertyMetadata(State.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        public State CurrentState
        {
            get { return (State)GetValue(CurrentStateProperty); }
            set { SetValue(CurrentStateProperty, value); }
        }

        public Point actPos = new Point(0.0, 0.0);
        public Point prevPos = new Point(0.0, 0.0);
        public object actElt; //can keep in private scope, but here pretty convenient

        public event EventHandler<ROIDescriptor.LastEventArgs> LastROIDrawEvent;

        protected void UpdateLastROIDrawEvent(ROIDescriptor.LastEventArgs e)
        {
            this.LastROIDrawEvent?.Invoke(this, e);
        }

        public abstract ROIDescriptor.LastEventData GetLastDrawEventData();
        public abstract ROIDescriptor.Contour GetROIDescriptorContour();

        /* ANCHORS using System.Collections.ObjectModel; */
        private static readonly DependencyPropertyKey AnchorsPropertyKey = DependencyProperty.RegisterReadOnly(
            "Anchors",
            typeof(ObservableCollection<Anchor>),
            typeof(ROI),
            new PropertyMetadata(null));

        public static readonly DependencyProperty AnchorsProperty = AnchorsPropertyKey.DependencyProperty;

        public ObservableCollection<Anchor> Anchors
        {
            get { return (ObservableCollection<Anchor>)GetValue(AnchorsProperty); }
            protected set { SetValue(AnchorsPropertyKey, value); }
        }


        public ROI()
        {
            Anchors = new ObservableCollection<Anchor>();
            base.ItemsSource = Anchors;
        }
    }
}
