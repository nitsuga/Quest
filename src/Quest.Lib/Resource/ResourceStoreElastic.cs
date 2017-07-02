using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.DataModel;
using Quest.Lib.Notifier;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Lib.Resource
{
    public class ResourceStoreElastic : IResourceStore
    {
        public QuestResource Get(int fleetno)
        {
            using (var db = new QuestEntities())
            {
                var dbinc = db.Resources.FirstOrDefault(x => x.FleetNo == fleetno);
                var res = Cloner.CloneJson<QuestResource>(dbinc);
                return res;
            }
        }

        public QuestResource Get(string callsign)
        {
            using (var db = new QuestEntities())
            {
                var dbinc = db.Resources.FirstOrDefault(x => x.Callsign.Callsign1 == callsign);
                var res = Cloner.CloneJson<QuestResource>(dbinc);
                return res;
            }
        }

        public int GetOffroadStatusId()
        {
            using (var db = new QuestEntities())
            {
                var status = db.ResourceStatus.FirstOrDefault(x => x.Offroad == true);
                return status.ResourceStatusID;
            }
        }
        
        public QuestResource Update(ResourceUpdate item)
        {
            using (var db = new QuestEntities())
            {
                var dbinc = db.Resources.FirstOrDefault(x => x.FleetNo == item.FleetNo);
                if (dbinc == null)
                {
                    dbinc = new DataModel.Resource();
                    db.Resources.Add(dbinc);

                    // use the tiestamp of the message for the creation time
                    //dbinc.Created = new DateTime((item.Timestamp + 62135596800) * 10000000);
                }

               //TODO:
                db.SaveChanges();
                var inc = Cloner.CloneJson<QuestResource>(dbinc);
                return inc;
            }
        }
    }
}
