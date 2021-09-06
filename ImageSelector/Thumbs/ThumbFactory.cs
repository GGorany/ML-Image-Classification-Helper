using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageSelector
{
    internal class ThumbFactory
    {
        /// <summary>
        /// Available thumbs positions
        /// </summary>
        public enum ThumbPosition
        {
            TopLeft,
            BottomRight,
        }

        /// <summary>
        /// Thumb factory
        /// </summary>
        /// <param name="thumbPosition">Thumb positions</param>
        /// <param name="canvas">Parent UI element that we will attach thumb as child</param>
        /// <param name="size">Size of thumb</param>
        /// <returns></returns>
        public static ThumbRect CreateThumbRect(ThumbPosition thumbPosition, Canvas canvas, double size)
        {
            ThumbRect thumbRect = new ThumbRect(size)
            {
                Cursor = GetCursor(thumbPosition),
                Visibility = Visibility.Hidden
            };
            canvas.Children.Add(thumbRect);
            return thumbRect;
        }

        public static ThumbCircle CreateThumbCircle(Canvas canvas, double size)
        {
            ThumbCircle thumbCircle = new ThumbCircle(size)
            {
                Cursor = Cursors.SizeAll,
                Visibility = Visibility.Hidden
            };
            canvas.Children.Add(thumbCircle);
            return thumbCircle;
        }

        /// <summary>
        /// Display proper cursor to corresponding thumb
        /// </summary>
        /// <param name="thumbPosition">Thumb position</param>
        /// <returns></returns>
        private static Cursor GetCursor(ThumbPosition thumbPosition)
        {
            return thumbPosition switch
            {
                (ThumbPosition.TopLeft) => Cursors.SizeNWSE,
                (ThumbPosition.BottomRight) => Cursors.SizeNWSE,
                _ => null,
            };
        }
    }
}
