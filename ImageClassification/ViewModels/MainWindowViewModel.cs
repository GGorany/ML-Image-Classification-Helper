using ImageClassification.Views;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace ImageClassification.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        #region Properties
        private string _Title = "Deep Learning Image Classification for AI Vision - Last Update : 2021.08.27";
        public string Title {
            get { return _Title; }
            set { SetProperty(ref _Title, value); }
        }
        #endregion

        #region Commands
        public DelegateCommand<string> NavigateCommand { get; private set; }

        private DelegateCommand _OpenModelFolder;
        public DelegateCommand OpenModelFolder => _OpenModelFolder ?? (_OpenModelFolder = new DelegateCommand(OnOpenModelFolder));
        #endregion

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateCommand = new DelegateCommand<string>(Navigate);

            _regionManager.RegisterViewWithRegion("MainPageRegion", typeof(TrainPage));
            _regionManager.RegisterViewWithRegion("MainPageRegion", typeof(PredictPage));
        }

        #region private Method
        private void Navigate(string navigatePath)
        {
            if (navigatePath != null)
                _regionManager.RequestNavigate("MainPageRegion", navigatePath);
        }

        private void OnOpenModelFolder()
        {
            string diPath = Directory.GetCurrentDirectory();

            try
            {
                Process.Start("explorer.exe", diPath);
            }
            catch (Win32Exception win32Exception)
            {
                Debug.WriteLine(win32Exception.Message);
            }
        }
        #endregion
    }
}
