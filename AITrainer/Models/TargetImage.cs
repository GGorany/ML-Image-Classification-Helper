
namespace AITrainer.Models
{
    public sealed class TargetImageFile
    {
        public string FileName { get; set; }
        public string FullFileName { get; set; }

        public TargetImageFile(string fileName, string fullFileName)
        {
            FileName = fileName;
            FullFileName = fullFileName;
        }
    }
}
