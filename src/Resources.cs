using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Localization;

namespace Dnn.Resx
{
    public class Resources 
    {
        Func<XmlDocument, IEnumerable<string>> keys = Functions.Keys;
        Func<string, XmlDocument> load              = Functions.Load;
        Func<string, string> normalizePath          = Functions.NormalizePath;
        Func<string, string> mapPath                = PathUtils.Instance.MapPath;
        Func<string, string, string> localize       = Localization.GetString;

        public Resources () { }
        
        public Resources (Func<XmlDocument, IEnumerable<string>> keys, Func<string, XmlDocument> load, Func<string, string> normalizePath, Func<string, string> mapPath, Func<string, string, string> localize)
        {
            this.keys = keys;
            this.load = load;
            this.normalizePath = normalizePath;
            this.mapPath = mapPath;
            this.localize = localize;
        }

        public IDictionary<string,string> For (string resKey )
        {
            return keys(load(mapPath(normalizePath(resKey ))))
                   .Select(key => new KeyValuePair<string, string>(key, localize(key, resKey)))
                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
