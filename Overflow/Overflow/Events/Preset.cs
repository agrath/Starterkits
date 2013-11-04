namespace Overflow.Events
{
    public struct Crop
    {
        public int X;
        public int Y;
        public int X2;
        public int Y2;

        public Crop(int x, int y, int x2, int y2)
        {
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
        }
    }

    public struct Preset
    {

        public string Name;
        public int TargetWidth;
        public int TargetHeight;
        public bool KeepAspect;
        public string PositionH;
        public string PositionV;

        public float Aspect
        {
            get { return (float)TargetWidth / TargetHeight; }
        }

        public Crop Fit(ImageInfo imageInfo)
        {
            Crop crop;

            if (TargetWidth == 0 || TargetHeight == 0)
            {
                crop.X = 0;
                crop.X2 = imageInfo.Width;
                crop.Y = 0;
                crop.Y2 = imageInfo.Height;
            }
            else if (Aspect >= imageInfo.Aspect)
            {
                // crop widest
                // relevant positioning: center top, center center, center bottom

                var height = ((float)imageInfo.Width / TargetWidth) * TargetHeight;

                crop.X = 0;
                crop.X2 = imageInfo.Width;

                switch (PositionV)
                {
                    case "T":
                        crop.Y = 0;
                        crop.Y2 = (int)height;
                        break;
                    case "B":
                        crop.Y = imageInfo.Height - (int)height;
                        crop.Y2 = imageInfo.Height;
                        break;
                    default: // CC
                        crop.Y = (int)(imageInfo.Height - height) / 2;
                        crop.Y2 = (int)(crop.Y + height);
                        break;
                }
            }
            else
            {
                // image widest
                // relevant positioning: left/right center, left/right top, left/right bottom

                var width = ((float)imageInfo.Height / TargetHeight) * TargetWidth;

                crop.Y = 0;
                crop.Y2 = imageInfo.Height;

                switch (PositionH)
                {
                    case "L":
                        crop.X = 0;
                        crop.X2 = (int)width;
                        break;
                    case "R":
                        crop.X = imageInfo.Width - (int)width;
                        crop.X2 = imageInfo.Width;
                        break;
                    default: // CC
                        crop.X = (int)(imageInfo.Width - width) / 2;
                        crop.X2 = (int)(crop.X + width);
                        break;
                }

            }

            return crop;
        }

        public Preset(string name, int targetWidth, int targetHeight, bool keepAspect, string positionH, string positionV)
        {
            Name = name;
            TargetWidth = targetWidth;
            TargetHeight = targetHeight;
            KeepAspect = keepAspect;
            PositionH = positionH;
            PositionV = positionV;
        }
    }
}