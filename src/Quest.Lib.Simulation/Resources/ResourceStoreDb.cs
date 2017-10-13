using Quest.Common.Simulation;
using Quest.Lib.Simulation.DataModelSim;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.Research.DataModelResearch;
using System;
using Quest.Lib.Data;

namespace Quest.Lib.Simulation.Resources
{
    public class ResourceStoreDb: IResourceStore
    {
        private IDatabaseFactory _dbFactory;

        public ResourceStoreDb(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public List<Avls> GetHistoricResources(DateTime lastCallsign, int take, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        //public List<Avl> GetHistoricResources(DateTime lastCallsign, int take, DateTime from, DateTime to)
        //{
        //    using (var SimData = new QuestDataEntities())
        //    {
        //        // get 'quantity' Resources from a specific Resource number
        //        List<Avl> data;
        //        if (lastCallsign != DateTime.MinValue)
        //            data = SimData.Avls.OrderBy(x => x.AvlsDateTime).Where(x => x.AvlsDateTime > lastCallsign).Take(take).ToList();
        //        else
        //            data = SimData.Avls.OrderBy(x => x.AvlsDateTime).Where(x => x.AvlsDateTime >= from && x.AvlsDateTime <= to).Take(take).ToList();

        //        return data;
        //    }
        //}

        public List<SimVehicle> GetVehicles()
        {
            return _dbFactory.Execute<QuestSimContext, List<SimVehicle>>((db) =>
            {
                return db.Vehicles
                        .ToList()
                        .Select(x => new SimVehicle
                        {
                            VehicleId = x.VehicleId,
                            VehicleType = x.VehicleType.Name,
                            Position = new GeoAPI.Geometries.Coordinate(x.Easting ?? 0, x.Northing ?? 0)
                        })
                        .ToList();
            });
        }
    }
}
