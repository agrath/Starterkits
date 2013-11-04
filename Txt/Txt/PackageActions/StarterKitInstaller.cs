using System;
using System.Linq;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.interfaces;

namespace Txt.PackageActions
{
    public class TxtInstaller : IPackageAction
    {
        public bool Execute(string packageName, XmlNode xmlData)
        {

            try
            {
                var contentService = ApplicationContext.Current.Services.ContentService;
                var homeNode = contentService.GetRootContent().First();
                contentService.PublishWithChildren(homeNode);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error<TxtInstaller>("Error at execute TxtInstaller package action", exception);

                return false;
            }
        }

        public string Alias()
        {
            return "TxtInstaller";
        }

        public bool Undo(string packageName, XmlNode xmlData)
        {
            return true;
        }

        public XmlNode SampleXml()
        {
            const string sample = "<Action runat=\"install\" undo=\"false\" alias=\"TxtInstaller\"></Action>";
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