using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Overflow.Events
{
    public class EventHandlers : ApplicationEventHandler
    {
        public EventHandlers()
        {
            //MediaService.Saved += MediaServiceSaved;
        }

        static void MediaServiceSaved(IMediaService sender, Umbraco.Core.Events.SaveEventArgs<IMedia> e)
        {
            foreach (var media in e.SavedEntities)
            {
                foreach (var propertyType in media.PropertyTypes)
                {
                    var type = propertyType;

                    if (type.DataTypeId != new Guid(Constants.PropertyEditors.ImageCropper))
                        continue;

                    var preValues = UmbracoContext.Current.Application.Services.DataTypeService.GetPreValuesByDataTypeId(type.DataTypeDefinitionId).Single();
                    var config = new Config(preValues);

                    Property uploadFieldProperty = null;

                    foreach (var propType in media.PropertyTypes.Where(propType => propType.DataTypeId == new Guid(Constants.PropertyEditors.UploadField)))
                        uploadFieldProperty = media.Properties.FirstOrDefault(p => p.Alias == propType.Alias);

                    if (uploadFieldProperty == null || uploadFieldProperty.Value == null || string.IsNullOrWhiteSpace(uploadFieldProperty.Value.ToString()))
                        continue;

                    var imgUrl = uploadFieldProperty.Value.ToString();
                    var imgUrlWithoutExtension = imgUrl.Substring(0, imgUrl.LastIndexOf('.'));

                    Property cropProperty = null;

                    foreach (var propType in media.PropertyTypes.Where(propType => propType.Alias == type.Alias))
                        cropProperty = media.Properties.Single(p => p.Alias == propType.Alias);

                    if ((cropProperty == null ||
                         (cropProperty.Value != null && string.IsNullOrEmpty(cropProperty.Value.ToString()) == false &&
                          cropProperty.Value.ToString().Contains(imgUrlWithoutExtension + "_"))))
                        continue;

                    int imgWidth;
                    using (var img = new Bitmap(HttpContext.Current.Server.MapPath(imgUrl)))
                        imgWidth = img.Width;

                    if (imgWidth == 0)
                        continue;

                    var presets = config.Presets;
                    var sbRaw = new StringBuilder();
                    var imageInfo = new ImageInfo(imgUrl);
                    var cropIndex = 1;

                    foreach (var preset in presets)
                    {
                        Crop crop;

                        if (imageInfo.Exists)
                        {
                            crop = preset.Fit(imageInfo);
                        }
                        else
                        {
                            crop.X = 0;
                            crop.Y = 0;
                            crop.X2 = preset.TargetWidth;
                            crop.Y2 = preset.TargetHeight;
                        }

                        sbRaw.Append(String.Format("{0},{1},{2},{3}", crop.X, crop.Y, crop.X2, crop.Y2));

                        if (cropIndex < presets.Count)
                        {
                            sbRaw.Append(";");
                        }

                        if (config.GenerateImages)
                        {
                            GenerateCrop(preset, imgUrl, config, sbRaw);
                        }
                        cropIndex++;
                    }
                    var saveData = new SaveData(sbRaw.ToString());

                    media.SetValue(cropProperty.Alias, saveData.Xml(config, imageInfo));
                    UmbracoContext.Current.Application.Services.MediaService.Save(media, 0, false);
                }
            }
        }

        private static void GenerateCrop(Preset preset, string imgUrl, Config config, StringBuilder sbRaw)
        {
            if (string.IsNullOrEmpty(preset.Name))
                return;

            var imgInfo = new ImageInfo(imgUrl);

            var data = new SaveData(sbRaw.ToString());
            imgInfo.GenerateThumbnails(data, config);
        }
    }
}