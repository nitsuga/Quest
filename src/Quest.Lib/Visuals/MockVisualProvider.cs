#if false
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quest.Common.Messages;

namespace Quest.Lib.Visuals
{
    public class MockVisualProvider : IVisualProvider
    {
        public List<Visual> GetVisualsCatalogue(GetVisualsCatalogueRequest request)
        {
            var items = new List<Visual>()
            {
                new Visual() {Name = "Cat A Coverage", VisualType = "Coverage.Percent"},
                new Visual() {Name = "Cat A Waiting", VisualType = "Coverage.Waiting"},
                new Visual() {Name = "Cat C Coverage", VisualType = "Coverage.Percent"},
                new Visual() {Name = "Cat C Waiting", VisualType = "Coverage.Waiting"},
                new Visual() {Name = "Decision", VisualType = "Decision"},
                new Visual() {Name = "Search Audit", VisualType = "Search.Audit"},
            };

            return items;
        }

        public List<Visual> GetVisualsData(GetVisualsDataRequest request)
        {
            var startTime = new DateTime(2016, 8, 1, 07, 0, 0);

            //TODO: LD / MP - Discuss - I think that all timeline data should be loaded in this way....
            var items = new List<Visual>()
            {
                new Visual()
                {
                    Name = "L2016081100111",
                    VisualType = "Event.Status",
                    Data = new List<TimelineData>()
                    {
                        new TimelineData(1, new DateTime(2016, 8, 1, 07, 0, 0), null, "T0"),
                        new TimelineData(2, new DateTime(2016, 8, 1, 09, 10, 0), null, "T1"),
                        new TimelineData(3, new DateTime(2016, 8, 1, 09, 25, 0), null, "T2"),
                        new TimelineData(4, new DateTime(2016, 8, 1, 09, 50, 0), null, "T3"),
                        new TimelineData(5, new DateTime(2016, 8, 1, 10, 15, 0), null, "T4"),
                        new TimelineData(6, new DateTime(2016, 8, 1, 11, 55, 0), null, "T5"),
                   }
                },
                new Visual()
                {
                    Name = "A203",
                    VisualType = "Route",
                    Data = new List<TimelineData>()
                    {
                        new TimelineData(7, new DateTime(2016, 8, 1, 08, 0, 0), new DateTime(2016, 8, 1, 09, 0, 0), ResourceStatus.available.ToString()),
                        new TimelineData(8, new DateTime(2016, 8, 1, 10, 10, 0), new DateTime(2016, 8, 1, 10, 25, 0), ResourceStatus.enroute.ToString()),
                        new TimelineData(9, new DateTime(2016, 8, 1, 10, 25, 0), new DateTime(2016, 8, 1, 10, 50, 0), ResourceStatus.onsecene.ToString()),
                        new TimelineData(10, new DateTime(2016, 8, 1, 10, 50, 0), new DateTime(2016, 8, 1, 11, 15, 0), ResourceStatus.tohosp.ToString()),
                        new TimelineData(11, new DateTime(2016, 8, 1, 11, 15, 0), new DateTime(2016, 8, 1, 12, 55, 0), ResourceStatus.athosp.ToString()),
                        new TimelineData(12, new DateTime(2016, 8, 1, 12, 55, 0), new DateTime(2016, 8, 1, 15, 55, 0), ResourceStatus.available.ToString()),
                   }
                }

            };

            return items;
        }
    }
}
#endif