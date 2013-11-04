using System;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Web;

namespace Overflow.Events
{
    public class AddPortfolioMedia
    {
        public static void Execute()
        {
            var portfolioMediaPath = HttpContext.Current.Server.MapPath("~/media/umbPortFolio");
            
            if (Directory.Exists(portfolioMediaPath) == false || Directory.GetFiles(portfolioMediaPath).Any() == false) 
                return;

            var mediaService = UmbracoContext.Current.Application.Services.MediaService;

            var portfolioFolder = mediaService.CreateMedia("UmbPortfolio", -1, Constants.Conventions.MediaTypes.Folder);
            mediaService.Save(portfolioFolder);

            foreach (var file in Directory.GetFiles(portfolioMediaPath))
            {
                var fileName = file.Substring(file.LastIndexOf(@"\", StringComparison.Ordinal) + 1);
                var fileExtension = fileName.Substring(fileName.LastIndexOf(".", StringComparison.Ordinal) + 1);

                var media = mediaService.CreateMedia(fileName.Replace("." + fileExtension, ""), portfolioFolder, Constants.Conventions.MediaTypes.Image);

                var uploadFile = new MemoryStream();

                using (var fileStream = File.OpenRead(file))
                    fileStream.CopyTo(uploadFile);

                var memoryFile = new MemoryFile(uploadFile, MimeTypes.GetMimeType(fileExtension), file);

                media.SetValue(Constants.Conventions.Media.File, memoryFile);
                mediaService.Save(media);
            }

            var portfolioContentType = UmbracoContext.Current.Application.Services.ContentTypeService.GetAllContentTypes().Single(x => x.Alias == "umbPortfolio");

            var contentService = UmbracoContext.Current.Application.Services.ContentService;

            var portfolios = contentService.GetContentOfContentType(portfolioContentType.Id);
            foreach (var portfolio in portfolios)
            {
                portfolio.SetValue("portfolioMediaFolder", portfolioFolder.Id);
                contentService.Save(portfolio);
            }
        }
    }
}