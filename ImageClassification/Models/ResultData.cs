using Prism.Mvvm;

namespace ImageClassification.Models
{
    public class ResultData : BindableBase
    {
        #region Properties
        private double _Score;
        private string _SlotName;

        public double Score
        {
            get => _Score;
            set => SetProperty(ref _Score, value);
        }

        public string SlotName
        {
            get => _SlotName;
            set => SetProperty(ref _SlotName, value);
        }
        #endregion
    }
}
