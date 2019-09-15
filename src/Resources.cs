using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Dnn.Resx
{
    public class Resources 
    {
        readonly Func<XmlDocument, IEnumerable<string>> keys = Functions.Keys;
        readonly Func<string, XmlDocument> load              = Functions.Load;
        readonly Func<string, string> normalizePath          = Functions.NormalizePath;
        readonly Func<string, Naming, string> naming         = Functions.ApplyNamingStrategy;
        readonly Func<string, string> mapPath                = DotNetNuke.Common.Utilities.PathUtils.Instance.MapPath;
        readonly Func<string, string, string> localize       = DotNetNuke.Services.Localization.Localization.GetString;
        
        public Resources () { }

        public Resources(Func<XmlDocument, IEnumerable<string>> keys, Func<string, XmlDocument> load, Func<string, string> normalizePath, Func<string, string> mapPath, Func<string, string, string> localize, Func<string, Naming, string> naming)
        {
            this.keys = keys;
            this.load = load;
            this.normalizePath = normalizePath;
            this.mapPath = mapPath;
            this.localize = localize;
            this.naming = naming;
        }

        public IDictionary<string, string> For(string resourceFile, Naming strategy)
        {
            return keys(load(mapPath(normalizePath(resourceFile))))
                   .ToDictionary(key => naming(key, strategy), key => localize(key, resourceFile));
        }
    }
}
