﻿using Quest.Common.Messages;
using Quest.Lib.Data;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using System.Linq;

namespace Quest.Lib.Resource
{
    public class ResourceStoreMssql : IResourceStore
    {
        IDatabaseFactory _dbFactory;

        public ResourceStoreMssql(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public QuestResource Get(int fleetno)
        {
            return _dbFactory.Execute<QuestContext, QuestResource>((db) =>
            {
                var dbinc = db.Resource.FirstOrDefault(x => x.FleetNo == fleetno);
                var res = Cloner.CloneJson<QuestResource>(dbinc);
                return res;
            });
        }

        public QuestResource Get(string callsign)
        {
            return _dbFactory.Execute<QuestContext, QuestResource>((db) =>
            {
                var dbinc = db.Resource.FirstOrDefault(x => x.Callsign.Callsign1 == callsign);
                var res = Cloner.CloneJson<QuestResource>(dbinc);
                return res;
            });
        }

        public int GetOffroadStatusId()
        {
            return _dbFactory.Execute<QuestContext, int>((db) =>
            {
                var status = db.ResourceStatus.FirstOrDefault(x => x.Offroad == true);
                return status.ResourceStatusId;
            });
        }
        
        public QuestResource Update(ResourceUpdate item)
        {
            return _dbFactory.Execute<QuestContext, QuestResource>((db) =>
            {
                var dbinc = db.Resource.FirstOrDefault(x => x.FleetNo == item.FleetNo);
                if (dbinc == null)
                {
                    dbinc = new DataModel.Resource();
                    db.Resource.Add(dbinc);

                    // use the tiestamp of the message for the creation time
                    //dbinc.Created = new DateTime((item.Timestamp + 62135596800) * 10000000);
                }

                //TODO:
                db.SaveChanges();
                var inc = Cloner.CloneJson<QuestResource>(dbinc);
                return inc;
            });
        }
    }
}
