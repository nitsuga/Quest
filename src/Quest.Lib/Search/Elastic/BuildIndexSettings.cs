using System;
using System.Collections.Generic;
using Quest.Lib.Utils;
using Nest;

namespace Quest.Lib.Search.Elastic
{
    public class BuildIndexSettings
    {
        public BuildIndexSettings(ElasticSettings settings, string defaultIndex, IDictionary<string, object> parameters)
        {
            Parameters = parameters;
            Logfrequency = 10000;
            DefaultIndex = defaultIndex;
            Settings = settings;

            LocalAreaNames = new PolygonManager();

            if (settings.LocalAreasFile.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                LocalAreaNames.BuildFromJson(settings.LocalAreasFile, 2);
            else if (settings.LocalAreasFile.EndsWith(".shp", StringComparison.InvariantCultureIgnoreCase))
                LocalAreaNames.BuildFromShapefile(settings.LocalAreasFile);

            MasterArea = new PolygonManager();
            MasterArea.BuildFromShapefile(settings.MasterAreaFile);

            Client = ElasticClientFactory.CreateClient(Settings);
        }

        public PolygonManager LocalAreaNames;
        public string DefaultIndex;
        public int Logfrequency;
        public PolygonManager MasterArea;
        public ElasticSettings Settings;
        public ElasticClient Client;
        public long RecordsTotal;
        public long RecordsCurrent;
        public long Indexed;
        public long Skipped;
        public long Errors;
        public bool RestrictToMaster;
        public DateTime StartedIndexing = DateTime.MinValue;
        public DateTime EstimateCompleteIndexing = DateTime.MinValue;
        public string[] DecompoundList = new [] { "marsh", "point", "hill", "field", "land", "gate", "hurst", "end" };

        public int RecordsPerSecond { get; set; }
        public IDictionary<string, object> Parameters;
    }
}
