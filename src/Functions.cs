using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Xml;


namespace Dnn.Resx
{
    public static class Functions
    {
        public static IEnumerable<string> Keys (this XmlDocument doc)
        {
           return
                doc
                .DocumentElement
                .SelectNodes("/root/data/@name")
                .Cast<XmlAttribute>()
                .Select(attr => attr.Value);
        }

        public static XmlDocument Load (this string filePath)
        {
            var doc = new XmlDocument();
            doc.Load(filePath);
            return doc;          
        }

        public static string NormalizePath(this string rescource)
        {
            rescource = rescource.ToLowerInvariant();
            rescource = rescource.StartsWith("desktopmodules") || rescource.StartsWith("admin") || rescource.StartsWith("controls")
                        ? "~/" + rescource : rescource;
            rescource = rescource.EndsWith(".resx")
                        ? rescource : rescource + ".resx";
            return rescource;
        }
    }
}
