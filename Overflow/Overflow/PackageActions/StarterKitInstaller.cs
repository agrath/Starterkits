using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.interfaces;
using Umbraco.Web;

namespace Overflow.PackageActions
{
    public class OverflowInstaller : IPackageAction
    {
        public bool Execute(string packageName, XmlNode xmlData)
        {
            try
            {
                AddFeaturedMedia();
                AddPortfolioMedia();

                var contentService = ApplicationContext.Current.Services.ContentService;
                var homeNode = contentService.GetRootContent().First();
                contentService.PublishWithChildren(homeNode);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error<OverflowInstaller>("Error at execute OverflowInstaller package action", exception);

                return false;
            }
        }

        private static void AddFeaturedMedia()
        {
            var featuresMediaPath = HttpContext.Current.Server.MapPath("~/media/umbFeatures");

            if (Directory.Exists(featuresMediaPath) == false || Directory.GetFiles(featuresMediaPath).Any() == false)
                return;

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

        private static void AddPortfolioMedia()
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

        public string Alias()
        {
            return "OverflowInstaller";
        }

        public bool Undo(string packageName, XmlNode xmlData)
        {
            return true;
        }

        public XmlNode SampleXml()
        {
            const string sample = "<Action runat=\"install\" undo=\"false\" alias=\"OverflowInstaller\"></Action>";
            return ParseStringToXmlNode(sample);
        }

        private static XmlNode ParseStringToXmlNode(string value)
        {
            var xmlDocument = new XmlDocument();
            var xmlNode = AddTextNode(xmlDocument, "error", "");

            try
            {
                xmlDocument.LoadXml(value);
                return xmlDocument.SelectSingleNode(".");
            }
            catch
            {
                return xmlNode;
            }
        }

        private static XmlNode AddTextNode(XmlDocument xmlDocument, string name, string value)
        {
            var node = xmlDocument.CreateNode(XmlNodeType.Element, name, "");
            node.AppendChild(xmlDocument.CreateTextNode(value));
            return node;
        }
    }
}