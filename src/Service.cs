using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Dnn.Resx
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute imr)
        {
            imr.MapHttpRoute("Dnn.Resx", "", "{controller}/{action}", new string[] { "Dnn.Resx" });
        }
    }

    public class ServiceController: DnnApiController
    {
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(string resKey)
        {
            var result = new Rescource().For(resKey);
            return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(result));
        }
    }
}
