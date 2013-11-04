using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Web;

namespace Overflow.Events
{
    public class AddFeaturedMedia
    {
        public static void Execute()
        {
            var featuresMediaPath = HttpContext.Current.Server.MapPath("~/media/umbFeatures");
            
            if (Directory.Exists(featuresMediaPath) == false || Directory.GetFiles(featuresMediaPath).Any() == false) 
                return;

            var mediaService = UmbracoContext.Current.Application.Services.MediaService;

            var featuresFolder = mediaService.CreateMedia("UmbFeatures", -1, Constants.Conventions.MediaTypes.Folder);
            mediaService.Save(featuresFolder);
                
            var featuresContentType = UmbracoContext.Current.Application.Services.ContentTypeService.GetAllContentTypes().Single(x => x.Alias == "umbFeature");
            var contentService = UmbracoContext.Current.Application.Services.ContentService;
            var features = contentService.GetContentOfContentType(featuresContentType.Id).ToList();            

            for (var i = 0; i < Directory.GetFiles(featuresMediaPath).Length; i++)
            {
                var file = Directory.GetFiles(featuresMediaPath)[i];
                var relativePath = "/" + file.Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], string.Empty).Replace(@"\", "/");

                var feature = features.Skip(i).Take(1).SingleOrDefault();
                if (feature == null) 
                    continue;

                feature.SetValue("image", relativePath);
                contentService.Save(feature);
            }
        }
    }
}