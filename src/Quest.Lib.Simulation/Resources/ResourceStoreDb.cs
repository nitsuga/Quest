using Quest.Common.Simulation;
using Quest.Lib.Simulation.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Lib.Simulation.Resources
{
    public class ResourceStoreDb: IResourceStore
    {
        public List<Avl> GetHistoricResources(DateTime lastResourceId, int take, DateTime from, DateTime to)
        {
            using (var SimData = new QuestDataEntities())
            {
                // get 'quantity' Resources from a specific Resource number
                List<Avl> data;
                if (lastResourceId != DateTime.MinValue)
                    data = SimData.Avls.OrderBy(x => x.AvlsDateTime).Where(x => x.AvlsDateTime > lastResourceId).Take(take).ToList();
                else
                    data = SimData.Avls.OrderBy(x => x.AvlsDateTime).Where(x => x.AvlsDateTime >= from && x.AvlsDateTime <= to).Take(take).ToList();

                return data;
            }
        }

        public List<SimVehicle> GetVehicles()
        {
            using (var db = new QuestSimEntities())
            {
                return db.VehicleViews
                        .ToList()
                        .Select(x=> new SimVehicle
                            {
                                VehicleId = x.VehicleId,
                                VehicleType = x.VehicleType,
                                Position = new GeoAPI.Geometries.Coordinate(x.Easting ?? 0, x.Northing ?? 0)
                            })
                        .ToList();
            }
        }
    }
}
