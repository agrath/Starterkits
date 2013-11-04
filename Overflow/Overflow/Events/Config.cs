using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Overflow.Events
{public class Config
    {
        public string UploadPropertyAlias { get; set; }
        public bool GenerateImages { get; set; }
        public int Quality { get; set; }
        public bool ShowLabel { get; set; }
        public List<Preset> Presets { get; set; }

        public Config(string configuration)
        {
            Presets = new List<Preset>();

            var configData = configuration.Split('|');

            if (configData.Length != 2) 
                return;

            var generalSettings = configData[0].Split(',');

            UploadPropertyAlias = generalSettings[0];
            GenerateImages = generalSettings[1] == "1";
            ShowLabel = generalSettings[2] == "1";

            int quality;
            Quality = generalSettings.Length >= 4 && Int32.TryParse(generalSettings[3], out quality) ? quality : 90;

            var presetData = configData[1].Split(';');

            foreach (var p in presetData.Select(preset => preset.Split(',')))
            {
                int targetWidth, targetHeight;

                if (p.Length < 4 || int.TryParse(p[1], out targetWidth) == false || int.TryParse(p[2], out targetHeight) == false)
                    continue;

                char[] cropPosition = { 'C', 'M' };

                if (p.Length >= 5)
                {
                    cropPosition = p[4].ToCharArray();
                }

                Presets.Add(new Preset(p[0], targetWidth, targetHeight, p[3] == "1", cropPosition[0].ToString(CultureInfo.InvariantCulture), cropPosition[1].ToString(CultureInfo.InvariantCulture)));
            }
        }
    }
}