using System.Windows;

namespace ImageSelector.Anchors
{
    public class Anchor : FrameworkElement
    {
        #region DependencyProperties
        public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.Register(
            "CurrentState",
            typeof(State),
            typeof(Anchor),
            new FrameworkPropertyMetadata(State.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            "Position",
            typeof(Point),
            typeof(Anchor),
            new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender));

        public State CurrentState
        {
            get { return (State)GetValue(CurrentStateProperty); }
            set { SetValue(CurrentStateProperty, value); }
        }

        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }
        #endregion

        public Anchor() { }
    }
}
