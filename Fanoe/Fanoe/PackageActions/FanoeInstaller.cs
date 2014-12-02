using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Fanoe.PackageActions
{
    public class FanoeInstaller : IPackageAction
    {
        public bool Execute(string packageName, XmlNode xmlData)
        {
            try
            {
                CopyGridConfiguration();

                var contentService = ApplicationContext.Current.Services.ContentService;
                var homeNode = contentService.GetRootContent().First();
                contentService.PublishWithChildren(homeNode);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error<FanoeInstaller>("Error at execute FanoeInstaller package action", exception);

                return false;
            }
        }

        private static void CopyGridConfiguration()
        {
            var fanoeConfigPath = HttpContext.Current.Server.MapPath("~/App_Plugins/Grid/Config/FanoeConfig.txt");
            var gridConfigPath = HttpContext.Current.Server.MapPath("~/config/grid.editors.config.js");

            try
            {
                File.Copy(fanoeConfigPath, gridConfigPath, true);
            }
            catch (Exception exception)
            {
                LogHelper.Error<FanoeInstaller>("Error at CopyGridConfiguration FanoeInstaller package action", exception);
            }
        }

        public string Alias()
        {
            return "FanoeInstaller";
        }

        public bool Undo(string packageName, XmlNode xmlData)
        {
            return true;
        }

        public XmlNode SampleXml()
        {
            const string sample = "<Action runat=\"install\" undo=\"false\" alias=\"FanoeInstaller\"></Action>";
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