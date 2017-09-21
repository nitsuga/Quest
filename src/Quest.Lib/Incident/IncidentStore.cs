using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using System;
using System.Linq;

namespace Quest.Lib.Incident
{
    public class IncidentStoreMsssql : IIncidentStore
    {
        public QuestIncident Get(string id)
        {
            using (var db = new QuestContext())
            {
                var dbinc = db.Incident.FirstOrDefault(x => x.Serial == id);
                var inc = Cloner.CloneJson<QuestIncident>(dbinc);
                return inc;
            }
        }

        public QuestIncident Update(IncidentUpdate item)
        {
            using (var db = new QuestContext())
            {
                var dbinc = db.Incident.FirstOrDefault(x => x.Serial == item.Serial);
                if (dbinc == null)
                {
                    dbinc = new DataModel.Incident();
                    db.Incident.Add(dbinc);

                    // use the tiestamp of the message for the creation time
                    dbinc.Created = new DateTime((item.Timestamp + 62135596800) * 10000000);
                }

                dbinc.Serial = item.Serial;
                dbinc.Status = item.Status;
                dbinc.IncidentType = item.IncidentType;
                dbinc.Complaint = item.Complaint;
                dbinc.Longitude = (float)item.Longitude;
                dbinc.Latitude = (float)item.Latitude;
                dbinc.Determinant = item.Determinant;
                dbinc.Location = item.Location;
                dbinc.Priority = item.Priority;
                dbinc.Sector = item.Sector;
                dbinc.IsClosed = false;
                dbinc.LastUpdated = DateTime.UtcNow;
                dbinc.LocationComment = item.LocationComment;
                dbinc.PatientAge = item.PatientAge;
                dbinc.PatientSex = item.PatientSex;
                db.SaveChanges();

                var inc = Cloner.CloneJson<QuestIncident>(dbinc);

                return inc;
            }
        }

        public void Close(string serial)
        {
            using (var db = new QuestContext())
            {
                var i = db.Incident.Where(x => x.Serial == serial).ToList();
                if (i.Any())
                {
                    foreach (var incident in i)
                    {
                        incident.IsClosed = true;
                        db.SaveChanges();
                    }
                }
            }
        }
    }

    
}
