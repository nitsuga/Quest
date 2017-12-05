using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Incident;
using Quest.Common.Utils;
using Quest.Lib.Data;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Lib.Incident
{
    public class IncidentStoreMsssql : IIncidentStore
    {
        IDatabaseFactory _dbFactory;

        public IncidentStoreMsssql(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public QuestIncident Get(string serial)
        {
            return _dbFactory.Execute<QuestContext, QuestIncident>((db) =>
            {
                var dbinc = db.Incident.FirstOrDefault(x => x.Serial == serial);
                var inc = Cloner.CloneJson<QuestIncident>(dbinc);
                return inc;
            });
        }

        public List<QuestIncident> GetIncidents(long revision, bool includeCatA = false, bool includeCatB = false)
        {
            return _dbFactory.Execute<QuestContext, List<QuestIncident>>((db) =>
            {
                var results = new List<DataModel.Incident>();

                if (includeCatA)
                    results.AddRange(db.Incident.Where(x => x.Priority.StartsWith("C1") && x.Revision > revision));

                if (includeCatB)
                    results.AddRange(db.Incident.Where(x => !x.Priority.StartsWith("C1") && x.Revision > revision));

                var features = new List<QuestIncident>();

                foreach (var inc in results)
                {
                    if (inc.Longitude != null)
                    {
                        if (inc.Latitude != null)
                        {
                            var incsFeature = new QuestIncident
                            {
                                Id = inc.IncidentId.ToString(),
                                EventId = inc.Serial,
                                Determinant = inc.Determinant,
                                Priority = inc.Priority,
                                Status = inc.Status,
                                DeterminantDescription = inc.DeterminantDescription,
                                Location = inc.Location,
                                LocationComment = inc.LocationComment,
                                PatientAge = inc.PatientAge,
                                PatientSex = inc.PatientSex,
                                ProblemDescription = inc.ProblemDescription,
                                Position = new LatLng(inc.Latitude ?? 0, inc.Longitude?? 0),
                            };

                            features.Add(incsFeature);
                        }
                    }
                }

                return features;
            });

        }

        public QuestIncident Update(IncidentUpdateRequest item)
        {
            return _dbFactory.Execute<QuestContext, QuestIncident>((db) =>
            {
                var dbinc = db.Incident.FirstOrDefault(x => x.Serial == item.Serial);
                if (dbinc == null)
                {
                    dbinc = new DataModel.Incident();
                    db.Incident.Add(dbinc);

                    // use the timestamp of the message for the creation time
                    dbinc.Created = Time.UnixTime(item.Timestamp);
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
            });
        }

        public void Close(string serial)
        {
            _dbFactory.Execute<QuestContext>((db) =>
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
            });
        }
    }    
}
