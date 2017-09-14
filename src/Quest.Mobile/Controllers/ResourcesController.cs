using Quest.Common.Messages;
using Quest.Mobile.Models;
using Quest.Mobile.Service;
using System.Web.Http;

namespace Quest.Mobile.Controllers
{
    public class ResourcesController : ApiController
    {
        private ResourceService _resourceService;

        public ResourcesController(ResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpGet]
        [ActionName("Get.geojson")]
        public ResourceFeatureCollection GetGeoJson(bool avail = true, bool busy = true)
        {
            return _resourceService.GetResources(avail, busy);
        }

        [HttpGet]
        public MapItemsResponse GetMapItems(bool Hospitals = false,
                bool IncidentsImmediate = true,
                bool IncidentsOther = true,
                bool ResourcesAvailable = true,
                bool ResourcesBusy = true,
                int Revision = 0,
                bool Standby = true,
                bool Stations = true)
        {
            MapItemsRequest request = new MapItemsRequest()
            {
                Hospitals = Hospitals,
                IncidentsImmediate = IncidentsImmediate,
                IncidentsOther = IncidentsOther,
                ResourcesAvailable = ResourcesAvailable,
                ResourcesBusy = ResourcesBusy,
                Revision = Revision,
                Standby = Standby,
                Stations = Stations
            };

            return _resourceService.GetMapItems(request);
        }
    }
}