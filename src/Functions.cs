using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Xml;


namespace Dnn.Resx
{
    public static class Functions
    {
        public static IEnumerable<string> Keys(this XmlDocument doc)
        {
            return
                 doc
                 .DocumentElement
                 .SelectNodes("/root/data/@name")
                 .Cast<XmlAttribute>()
                 .Select(attr => attr.Value);
        }

        public static XmlDocument Load(this string filePath)
        {
            var doc = new XmlDocument();
            doc.Load(filePath);
            return doc;
        }

        public static string NormalizePath(this string resource)
        {
            resource = resource.ToLowerInvariant();
            resource = resource.StartsWith("desktopmodules") || resource.StartsWith("admin") || resource.StartsWith("controls")
                        ? "~/" + resource : resource;
            resource = resource.EndsWith(".resx")
                        ? resource : resource + ".resx";
            return resource;
        }
    }
}
