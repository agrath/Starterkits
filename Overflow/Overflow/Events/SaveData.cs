using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Overflow.Events
{
    public class SaveData
    {
        public ArrayList Data { get; set; }

        public SaveData(string raw)
        {
            Data = new ArrayList();

            var crops = raw.Split(';');

            foreach (var val in crops.Select(crop => crop.Split(',')))
            {
                Data.Add(
                    new Crop(
                        Convert.ToInt32(val[0]),
                        Convert.ToInt32(val[1]),
                        Convert.ToInt32(val[2]),
                        Convert.ToInt32(val[3])
                        )
                    );
            }
        }

        public string Xml(Config config, ImageInfo imageInfo)
        {
            var doc = CreateBaseXmlDocument();
            var root = doc.DocumentElement;

            if (root == null) return null;

            var dateStampNode = doc.CreateNode(XmlNodeType.Attribute, "date", null);
            dateStampNode.Value = imageInfo.DateStamp.ToString("s");
            root.Attributes.SetNamedItem(dateStampNode);

            for (var i = 0; i < Data.Count; i++)
            {
                var crop = (Crop)Data[i];
                var preset = config.Presets[i];

                var newNode = doc.CreateElement("crop");

                var nameNode = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                nameNode.Value = preset.Name;
                newNode.Attributes.SetNamedItem(nameNode);

                var xNode = doc.CreateNode(XmlNodeType.Attribute, "x", null);
                xNode.Value = crop.X.ToString(CultureInfo.InvariantCulture);
                newNode.Attributes.SetNamedItem(xNode);

                var yNode = doc.CreateNode(XmlNodeType.Attribute, "y", null);
                yNode.Value = crop.Y.ToString(CultureInfo.InvariantCulture);
                newNode.Attributes.SetNamedItem(yNode);

                var x2Node = doc.CreateNode(XmlNodeType.Attribute, "x2", null);
                x2Node.Value = crop.X2.ToString(CultureInfo.InvariantCulture);
                newNode.Attributes.SetNamedItem(x2Node);

                var y2Node = doc.CreateNode(XmlNodeType.Attribute, "y2", null);
                y2Node.Value = crop.Y2.ToString(CultureInfo.InvariantCulture);
                newNode.Attributes.SetNamedItem(y2Node);

                if (config.GenerateImages)
                {
                    var urlNode = doc.CreateNode(XmlNodeType.Attribute, "url", null);
                    urlNode.Value = String.Format("{0}/{1}_{2}.jpg",
                                                  imageInfo.RelativePath.Substring(0, imageInfo.RelativePath.LastIndexOf('/')),
                                                  imageInfo.Name,
                                                  preset.Name);
                    newNode.Attributes.SetNamedItem(urlNode);
                }

                root.AppendChild(newNode);
            }

            return doc.InnerXml;
        }

        private static XmlDocument CreateBaseXmlDocument()
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("crops");
            doc.AppendChild(root);
            return doc;
        }
    }
}