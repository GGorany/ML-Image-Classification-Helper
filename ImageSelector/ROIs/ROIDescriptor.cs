using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ImageSelector.ROIs
{
    public class ROIDescriptor
    {
        public class Contour
        {
            public ROItype roiType;
            public List<Point> points;

            public Contour()
            {
                points = new List<Point>();
            }

            public bool IsChanged(Contour other)
            {
                if (other != null && roiType.Equals(other.roiType))
                {
                    return points.SequenceEqual(other.points);
                }
                return false;
            }
        }

        public class LastEventData
        {
            public EventType type;
            public EventTool tool;
            public ROItype roi;
            public List<Point> coordinates;
            public List<double> otherParameters;
            public bool IsChanged(LastEventData other)
            {
                if (other != null && type.Equals(other.type) && tool.Equals(other.tool) && coordinates.SequenceEqual(other.coordinates))
                {
                    return otherParameters.SequenceEqual(other.otherParameters);
                }
                return false;
            }
        }

        public class LastEventArgs : EventArgs
        {
            public LastEventData data;

            public LastEventArgs(LastEventData data)
            {
                this.data = data;
            }
        }

        public List<double> boundingBox;

        public List<Contour> contours;

        public ROIDescriptor()
        {
            boundingBox = new List<double>();
            contours = new List<Contour>();
        }

        public bool IsChanged(ROIDescriptor other)
        {
            if (other == null)
                return false;

            if (boundingBox != null && !boundingBox.SequenceEqual(other.boundingBox))
                return false;

            if (boundingBox == null && other.boundingBox != null)
                return false;

            if (contours.Count != other.contours.Count)
                return false;

            for (int i = 0; i < contours.Count; i++)
                if (!contours[i].IsChanged(other.contours[i]))
                    return false;

            return true;
        }
    }
}
