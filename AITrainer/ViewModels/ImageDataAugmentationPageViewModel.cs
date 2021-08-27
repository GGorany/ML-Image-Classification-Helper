using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

using Keras.PreProcessing.Image;
using System.Threading.Tasks;
using Keras;
using Keras.Models;

namespace AITrainer.ViewModels
{
    public class ImageDataAugmentationPageViewModel : BindableBase
    {
        private List<FileInfo> originalFileInfos = new List<FileInfo>();

        #region Properties
        private string _OriginalImageFolderName = string.Empty;
        private int _OriginalImageCount = 0;
        private string _ProcessedImageFolderName = string.Empty;
        private string _Status = string.Empty;

        public string OriginalImageFolderName
        {
            get => _OriginalImageFolderName;
            set => SetProperty(ref _OriginalImageFolderName, value);
        }

        public int OriginalImageCount
        {
            get => _OriginalImageCount;
            set => SetProperty(ref _OriginalImageCount, value);
        }

        public string ProcessedImageFolderName
        {
            get => _ProcessedImageFolderName;
            set => SetProperty(ref _ProcessedImageFolderName, value);
        }

        public string Status
        {
            get => _Status;
            set => SetProperty(ref _Status, value);
        }
        #endregion

        #region Properties - For Keras Parameter
        private uint _ImageSize = 256;

        public uint ImageSize
        {
            get => _ImageSize;
            set => SetProperty(ref _ImageSize, value);
        }

        #endregion

        #region Commands
        private DelegateCommand _OpenOriginalImageFolder;
        private DelegateCommand _SelectProcessedImageFolder;
        private DelegateCommand _StartAugmentation;

        public DelegateCommand OpenOriginalImageFolder => _OpenOriginalImageFolder ??= new DelegateCommand(() =>
        {
            this.originalFileInfos.Clear();

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.OriginalImageFolderName = dialog.SelectedPath;

                DirectoryInfo di = new DirectoryInfo(this.OriginalImageFolderName);

                foreach (FileInfo File in di.GetFiles())
                {
                    if ((File.Extension.ToLower().CompareTo(".png") == 0) || (File.Extension.ToLower().CompareTo(".jpg") == 0))
                        this.originalFileInfos.Add(File);
                }
            }

            this.OriginalImageCount = this.originalFileInfos.Count;
        });

        public DelegateCommand SelectProcessedImageFolder => _SelectProcessedImageFolder ??= new DelegateCommand(() =>
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                this.ProcessedImageFolderName = dialog.SelectedPath;
        });

        public DelegateCommand StartAugmentation => _StartAugmentation ??= new DelegateCommand(OnStartAugmentation);
        #endregion

        #region Constructor
        public ImageDataAugmentationPageViewModel()
        {
            this.OriginalImageCount = 0;
        }
        #endregion

        #region Private Method
        private void OnStartAugmentation()
        {
            this.Status = string.Empty;

            //if (string.IsNullOrEmpty(this.OriginalImageFolderName))
            //{
            //    this.Status = "Plz, Set Original Image Folder.";
            //    return;
            //}

            //if (string.IsNullOrEmpty(this.ProcessedImageFolderName))
            //{
            //    this.Status = "Plz, Set Processed Image Save Folder.";
            //    return;
            //}

            //if (this.originalFileInfos.Count == 0)
            //{
            //    this.Status = "No Image File in Original Image Folder.";
            //    return;
            //}

            try
            {
                ImageDataGenerator imageDataGenerator = new ImageDataGenerator(
                    rescale: 1f / 255,
                    rotation_range: 20,
                    width_shift_range: 0.1f,
                    height_shift_range: 0.1f,
                    shear_range: 0.1f,
                    zoom_range: 0.1f,
                    horizontal_flip: true,
                    fill_mode: "nearest"
                    );

                //var iterator = imageDataGenerator.FlowFromDirectory(
                //    directory: this.OriginalImageFolderName,
                //    class_mode: "binary",
                //    color_mode: "rgb",
                //    target_size: ((int)this.ImageSize, (int)this.ImageSize).ToTuple(),
                //    save_to_dir: this.ProcessedImageFolderName,
                //    save_format: "jpg",
                //    seed: 42
                //    );

                KerasIterator iterator = imageDataGenerator.FlowFromDirectory(
                    directory: "D:/DHKim_Data/HighSpeedFlexiblePSP_Data/분류된데이타_AI모델/테스트폴더/original",
                    class_mode: "categorical",
                    color_mode: "rgb",
                    batch_size: 10,
                    target_size: new Tuple<int, int>((int)this.ImageSize, (int)this.ImageSize),
                    save_to_dir: "D:/DHKim_Data/HighSpeedFlexiblePSP_Data/분류된데이타_AI모델/테스트폴더/processed",
                    save_format: "jpeg"
                    );





                //public ImageDataGenerator(
                // bool featurewise_center = false,
                // bool samplewise_center = false,
                // bool featurewise_std_normalization = false,
                // bool samplewise_std_normalization = false,
                // bool zca_whitening = false,
                // float zca_epsilon = 1E-06F,
                // int rotation_range = 0,
                // float width_shift_range = 0,
                // float height_shift_range = 0,
                // float[] brightness_range = null,
                // float shear_range = 0,
                // float zoom_range = 0,
                // float channel_shift_range = 0,
                // string fill_mode = "nearest",
                // float cval = 0,
                // bool horizontal_flip = false,
                // bool vertical_flip = false,
                // float? rescale = null,
                // PyObject preprocessing_function = null,
                // string data_format = "",
                // float validation_split = 0,
                // string dtype = "");

                //public KerasIterator FlowFromDirectory(
                // string directory,
                // Tuple<int, int> target_size = null,
                // string color_mode = "rgb",
                // string[] classes = null,
                // string class_mode = "categorical",
                // int batch_size = 32,
                // bool shuffle = true,
                // int? seed = null,
                // string save_to_dir = "",
                // string save_prefix = "",
                // string save_format = "png",
                // bool follow_links = false,
                // string subset = "",
                // string interpolation = "nearest");
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                    this.Status = $"Msg : {ex.Message}";
                else
                    this.Status = $"Msg : {ex.Message} \nInnerMsg : {ex.InnerException.Message}";
                return;
            }

            this.Status = "Done.";
        }


        #endregion
    }
}
