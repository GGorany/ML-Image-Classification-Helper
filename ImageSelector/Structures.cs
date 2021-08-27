
namespace ImageSelector
{
    public enum ROItype
    {
        None,
        Line,
        Rectangle,
        Oval,
    }

    public enum EventType
    {
        NoEvent,
        Click,
        Draw,
    }

    public enum EventTool
    {
        None,
        Cursor,
        ROI,
        Pan
    }

    public enum State
    {
        DrawingInProgress,
        Normal,
        Selected
    }

    public enum AnchorType
    {
        Move,
        Resize
    }
}
