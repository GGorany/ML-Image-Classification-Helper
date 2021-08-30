using AITrainer.Models;

using Microsoft.ML;
using Microsoft.ML.Data;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace AITrainer.ViewModels
{
    public class PredictPageViewModel : BindableBase
    {
        MLContext mlContext = null;
        ITransformer loadedModel = null;
        PredictionEngine<InMemoryImageData, ImagePrediction> predictionEngine = null;

        private Mat _sourceMat = null;

        #region Properties
        private string _ModelFileName;
        private string _FolderName;
        private BitmapSource _OriginalImage;
        private BitmapSource _CroppedImage;
        private System.Windows.Point _TopLeftPoint = new System.Windows.Point(10, 10);
        private System.Windows.Point _BottomRightPoint = new System.Windows.Point(50, 50);
        private string _ResultText;
        private long _PridictTime;
        private ObservableCollection<TargetImageFile> _TargetImageFiles;
        private TargetImageFile _SelectedTargetImageFile;
        private ObservableCollection<ResultData> _Results;

        public string ModelFileName
        {
            get { return _ModelFileName; }
            set { SetProperty(ref _ModelFileName, value); }
        }

        public string FolderName
        {
            get { return _FolderName; }
            set { SetProperty(ref _FolderName, value); }
        }

        public BitmapSource OriginalImage
        {
            get { return _OriginalImage; }
            set { SetProperty(ref _OriginalImage, value); }
        }

        public BitmapSource CroppedImage
        {
            get { return _CroppedImage; }
            set { SetProperty(ref _CroppedImage, value); }
        }

        public System.Windows.Point TopLeftPoint
        {
            get { return _TopLeftPoint; }
            set 
            { 
                SetProperty(ref _TopLeftPoint, value);
                RunPredict();
            }
        }

        public System.Windows.Point BottomRightPoint
        {
            get { return _BottomRightPoint; }
            set 
            { 
                SetProperty(ref _BottomRightPoint, value);
                RunPredict();
            }
        }

        public string ResultText
        {
            get { return _ResultText; }
            set { SetProperty(ref _ResultText, value); }
        }

        public long PridictTime
        {
            get { return _PridictTime; }
            set { SetProperty(ref _PridictTime, value); }
        }

        public ObservableCollection<TargetImageFile> TargetImageFiles
        {
            get { return _TargetImageFiles; }
            set { SetProperty(ref _TargetImageFiles, value); }
        }

        public TargetImageFile SelectedTargetImageFile
        {
            get { return _SelectedTargetImageFile; }
            set
            {
                SetProperty(ref _SelectedTargetImageFile, value);
                OnFileSelected();
            }
        }

        public ObservableCollection<ResultData> Results
        {
            get { return _Results; }
            set { SetProperty(ref _Results, value); }
        }
        #endregion

        #region Commands
        private DelegateCommand _FolderSelect;
        private DelegateCommand _ModelFileSelect;

        public DelegateCommand FolderSelect => _FolderSelect ?? (_FolderSelect = new DelegateCommand(OnFolderSelect));
        public DelegateCommand ModelFileSelect => _ModelFileSelect ?? (_ModelFileSelect = new DelegateCommand(OnModelFileSelect));
        #endregion

        #region Constructor
        public PredictPageViewModel()
        {
            Results = new ObservableCollection<ResultData>();
            TargetImageFiles = new ObservableCollection<TargetImageFile>();
            mlContext = new MLContext(seed: 1);
        }
        #endregion

        #region private Method
        private void OnModelFileSelect()
        {
            try
            {
                ModelFileName = "";

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "ML.NET 모델 파일 (*.zip)|*.zip";
                dialog.ShowDialog();

                ModelFileName = dialog.FileName;

                Debug.WriteLine($"Loading model from: {ModelFileName}");

                // Load the model
                loadedModel = mlContext.Model.Load(ModelFileName, out DataViewSchema modelInputSchema);

                // Create prediction engine to try a single prediction (input = ImageData, output = ImagePrediction)
                predictionEngine = mlContext.Model.CreatePredictionEngine<InMemoryImageData, ImagePrediction>(loadedModel);

                //private static List<string> GetSlotNames(DataViewSchema schema, string name)
                List<string> SlotNames = GetSlotNames(predictionEngine.OutputSchema, "Score");

                Results.Clear();

                foreach (string slotname in SlotNames)
                    Results.Add(new ResultData() { Score = 0.0, SlotName = slotname });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void OnFolderSelect()
        {
            try
            {
                SelectedTargetImageFile = null;
                TargetImageFiles.Clear();

                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.ShowDialog();

                FolderName = dialog.SelectedPath;

                DirectoryInfo di = new DirectoryInfo(FolderName);

                foreach (FileInfo File in di.GetFiles())
                {
                    if ((File.Extension.ToLower().CompareTo(".png") == 0) || (File.Extension.ToLower().CompareTo(".jpg") == 0))
                        TargetImageFiles.Add(new TargetImageFile(File.Name, File.FullName));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void OnFileSelected()
        {
            if (SelectedTargetImageFile != null)
            {
                this._sourceMat = new Mat(SelectedTargetImageFile.FullFileName, ImreadModes.Unchanged);
                OriginalImage = this._sourceMat.ToBitmapSource();

                RunPredict();
            }
            else
            {
                OriginalImage = null;
                CroppedImage = null;
            }
        }

        private void RunPredict()
        {
            if (loadedModel != null)
            {
                // Measure #1 prediction execution time.
                Stopwatch watch = Stopwatch.StartNew();
                try
                {
                    Rect rect = GetRect();
                    if ((rect == Rect.Empty) || (rect.Size == Size.Zero))
                        return;

                    Mat _resultMat = _sourceMat.SubMat(rect);
                    CroppedImage = _resultMat.ToBitmapSource();

                    byte[] imagebyte;
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(CroppedImage));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        imagebyte = ms.ToArray();
                    }

                    InMemoryImageData inMemoryImageData = new InMemoryImageData(
                        image: imagebyte,
                        //image: File.ReadAllBytes(SelectedTargetImage.FullFileName),
                        label: SelectedTargetImageFile.FileName,
                        imageFileName: SelectedTargetImageFile.FullFileName
                    );

                    var prediction = predictionEngine.Predict(inMemoryImageData);

                    // Stop measuring time.
                    watch.Stop();
                    PridictTime = watch.ElapsedMilliseconds;

                    ResultText = prediction.PredictedLabel;

                    for (int i = 0; i < prediction.Score.Length; i++)
                        Results[i].Score = prediction.Score[i];
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private static List<string> GetSlotNames(DataViewSchema schema, string name)
        {
            var column = schema.GetColumnOrNull(name);

            var slotNames = new VBuffer<ReadOnlyMemory<char>>();
            column.Value.GetSlotNames(ref slotNames);
            var names = new string[slotNames.Length];
            var num = 0;
            foreach (var denseValue in slotNames.DenseValues())
            {
                names[num++] = denseValue.ToString();
            }

            return names.ToList();
        }

        private Rect GetRect()
        {
            Rect baserect = new Rect(0, 0, (int)this._sourceMat.Width, (int)this._sourceMat.Height);
            Point topLeft = new Point(TopLeftPoint.X, TopLeftPoint.Y);
            Point bottomRight = new Point(BottomRightPoint.X, BottomRightPoint.Y);

            if (!baserect.Contains(topLeft) && !baserect.Contains(bottomRight))
                return Rect.Empty;

            int x = (int)Math.Min(TopLeftPoint.X, BottomRightPoint.X);
            int y = (int)Math.Min(TopLeftPoint.Y, BottomRightPoint.Y);
            int w = (int)Math.Abs(BottomRightPoint.X - TopLeftPoint.X);
            int h = (int)Math.Abs(BottomRightPoint.Y - TopLeftPoint.Y);

            Rect rect = new Rect(x, y, w, h);

            if (x < 0)
            {
                rect.X = 0;
                rect.Width = w + x;
            }

            if (y < 0)
            {
                rect.Y = 0;
                rect.Height = h + y;
            }

            if ((x + w) > (int)this._sourceMat.Width)
                rect.Width = (int)this._sourceMat.Width - x;

            if ((rect.Y + rect.Height) > (int)this._sourceMat.Height)
                rect.Height = (int)this._sourceMat.Height - y;

            return rect;
        }
        #endregion
    }
}
