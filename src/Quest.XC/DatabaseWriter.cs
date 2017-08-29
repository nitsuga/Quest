using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using Quest.Lib;
using Quest.Common.Messages;

namespace Quest.XC
{
    public static class DatabaseWriter
    {
        public static void Save(this CloseIncident instance, int timeout, String channel)
        {
            using (QuestDataClassesDataContext db = new QuestDataClassesDataContext())
            {
                db.CloseIncident(instance.Serial);
            }
        }

        public static void Save(this DeleteResource instance, int timeout, String channel)
        {
            using (QuestDataClassesDataContext db = new QuestDataClassesDataContext())
            {
                db.DeleteResource(instance.Callsign);
            }
        }

        public static void Save(this BeginDump instance, int timeout, String channel)
        {
            using (QuestDataClassesDataContext db = new QuestDataClassesDataContext())
            {
            }
        }

        public static void Save(this IncidentUpdate instance, int timeout, String channel)
        {
            using (QuestDataClassesDataContext db = new QuestDataClassesDataContext())
            {
                db.AddIncident(instance.Serial, instance.Status, instance.IncidentType, instance.Complaint, instance.Geometry, instance.Determinant, instance.Location, instance.Priority, instance.Sector, instance.Description);
            }
        }

        public static void Save(this ResourceUpdate instance, int timeout, String channel)
        {
            using (QuestDataClassesDataContext db = new QuestDataClassesDataContext())
            {
                db.AddResource(
                    instance.Callsign, instance.ResourceType, instance.Status, instance.Geometry, instance.Speed,
                    instance.Direction, instance.Skill, instance.LastUpdate, instance.FleetNo, instance.Sector,
                    instance.Incident, instance.Emergency, instance.Destination, instance.Agency, instance.Class, instance.EventType );
            }
        }

        public static void Save(this ResourceLogon instance, int timeout, String channel)
        {
            using (QuestDataClassesDataContext db = new QuestDataClassesDataContext())
            {
                db.ResourceLogon(instance.Callsign, instance.Logon, instance.Logoff);
            }
        }

        public static List<MessageBroker.Objects.XCOutbound> GetOutboundList(int timeout)
        {
            String timestamp = DateTime.Now.ToString();

            using (QuestDataClassesDataContext db = new QuestDataClassesDataContext())
            {
                var list = db.XCOutbounds.Where(x => x.Completed == null).ToList();

                list.ForEach(x => x.Completed = timestamp);

                db.SaveChanges();

                return list.Select(x => new MessageBroker.Objects.XCOutbound() { channel = x.Channels, command = x.Command, XCOutboundId = x.XCOutboundId }).ToList();

            }

        }

    }
}
