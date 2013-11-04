using System;
using System.Linq;
using System.Web;
using System.Xml;
using Overflow.Events;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.interfaces;

namespace Overflow.PackageActions
{
    public class OverflowInstaller : IPackageAction
    {
        public bool Execute(string packageName, XmlNode xmlData)
        {

            try
            {
                TouchWebConfig();

                //AddPortfolioCropperToImageMediaType.Execute();
                AddFeaturedMedia.Execute();
                AddPortfolioMedia.Execute();

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

        public void TouchWebConfig()
        {
            var sWebConfigPath = HttpContext.Current.Server.MapPath("~/Web.Config");
            System.IO.File.SetLastWriteTimeUtc(sWebConfigPath, DateTime.UtcNow);
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