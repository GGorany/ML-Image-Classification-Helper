using Prism.Mvvm;

namespace AITrainer.Models
{
    public class ResultData : BindableBase
    {
        #region Properties
        private double _Score;
        public double Score
        {
            get => _Score;
            set => SetProperty(ref _Score, value);
        }

        private string _SlotName;
        public string SlotName
        {
            get => _SlotName;
            set => SetProperty(ref _SlotName, value);
        }
        #endregion
    }
}
