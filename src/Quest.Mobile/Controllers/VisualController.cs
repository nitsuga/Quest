#pragma warning disable 0169,649
using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
#if NET45
using System.Data.Spatial;
#endif
using Quest.Mobile.Attributes;
using Quest.Mobile.Service;
using Quest.Common.Messages;

namespace Quest.Mobile.Controllers
{

    public class VisualController : Controller
    {
         private VisualisationService _visualisationService;

        public VisualController(VisualisationService visualisationService)
        {
            _visualisationService=visualisationService;
        }

        //[Authorize(Roles = "administrator,user")]
        [HttpGet]
        [NoCache]
        public ActionResult GetCatalogue(string dateFrom, string dateTo, string resource, string incident, string[] visuals)
        {
            GetVisualsCatalogueRequest request = new GetVisualsCatalogueRequest()
            {
                DateFrom = DateTime.Parse(dateFrom),
                DateTo = DateTime.Parse(dateTo),
                Resource = resource,
                Incident = incident,
                Visuals = visuals
            };

            var visualisationResponse = _visualisationService.GetCatalogue(request);
            return Json(visualisationResponse, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "administrator,user")]
        [HttpGet]
        [NoCache]
        public ActionResult GetVisualsData(string visuals)
        {
            var visualisationResponse = _visualisationService.GetVisualsData(visuals.Split(',').ToList());

            // return the GeoJSON object
            //return Json(visualisationResponse.Geometry,JsonRequestBehavior.AllowGet);
            var js = JsonConvert.SerializeObject(visualisationResponse.Geometry);

            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }

        //[Authorize(Roles = "administrator,user")]
        [HttpGet]
        [NoCache]
        public ActionResult Query(string provider, string parameters)
        {
            var visualisationResponse = _visualisationService.Query(provider, parameters);
            //var r = Json(visualisationResponse, JsonRequestBehavior.AllowGet);
            JsonSerializerSettings settings = new JsonSerializerSettings() { MaxDepth =1000};
            
            var js = JsonConvert.SerializeObject(visualisationResponse, settings);

            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }

    }
}
