using System;

namespace ImageSelector.ROIs
{
	public class ROIValueChangedEventArgs : EventArgs
	{
		public ROIDescriptor.LastEventData lastEventData;
		public ROIDescriptor ROI;
		public ROIValueChangedEventArgs(ROIDescriptor.LastEventData lastEventData, ROIDescriptor oldROI, ROIDescriptor newROI)
		{
			this.lastEventData = lastEventData;
			this.ROI = newROI;
		}
	}
}
