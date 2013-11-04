using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
using umbraco.editorControls.imagecropper;

namespace Overflow.Events
{
    public class ImageInfo
    {
        public Image Image { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Aspect { get; set; }
        public DateTime DateStamp { get; set; }
        public string Path { get; set; }
        public string RelativePath { get; set; }

        public ImageInfo(string relativePath)
        {
            RelativePath = relativePath;
            Path = IOHelper.MapPath(relativePath);
            if (File.Exists(Path))
            {
                var fileName = Path.Substring(Path.LastIndexOf('\\') + 1);
                Name = fileName.Substring(0, fileName.LastIndexOf('.'));

                byte[] buffer;

                using (var fileStream = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, (int)fileStream.Length);
                    fileStream.Close();
                }

                try
                {
                    Image = Image.FromStream(new MemoryStream(buffer));

                    Width = Image.Width;
                    Height = Image.Height;
                    Aspect = (float)Width / Height;
                    DateStamp = File.GetLastWriteTime(Path);
                }
                catch (Exception)
                {
                    Width = 0;
                    Height = 0;
                    Aspect = 0;
                }
            }
            else
            {
                Width = 0;
                Height = 0;
                Aspect = 0;
            }
        }

        public bool Exists
        {
            get { return Width > 0 && Height > 0; }
        }

        public string Directory
        {
            get { return Path.Substring(0, Path.LastIndexOf('\\')); }
        }

        public void GenerateThumbnails(SaveData saveData, Config config)
        {
            if (config.GenerateImages == false) 
                return;

            for (var i = 0; i < config.Presets.Count; i++)
            {
                var crop = (Crop)saveData.Data[i];
                var preset = config.Presets[i];

                // Crop rectangle bigger than actual image
                if (crop.X2 - crop.X > Width || crop.Y2 - crop.Y > Height)
                {
                    crop = preset.Fit(this);
                }

                ImageTransform.Execute(
                    Path,
                    String.Format("{0}_{1}", Name, preset.Name),
                    crop.X,
                    crop.Y,
                    crop.X2 - crop.X,
                    crop.Y2 - crop.Y,
                    preset.TargetWidth,
                    preset.TargetHeight,
                    config.Quality
                    );
            }
        }
    }
}