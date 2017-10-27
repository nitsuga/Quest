using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Nest;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.Search.Indexers
{
    public class CameraIndexer : ElasticIndexer
    {
        private const string BASEURL = "http://www.tfl.gov.uk";
        public string URL { get; set; } = "http://content.tfl.gov.uk/camera-list.xml";

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        public static void Build(BuildIndexSettings config)
        {
            try
            {
                //https://s3-eu-west-1.amazonaws.com/jamcams.tfl.gov.uk/jamcams-camera-list.xml
                //var locationsRequest = BASEURL + "/tfl/livetravelnews/trafficcams/cctv/jamcams-camera-list.xml";
                //var locationsRequest = "https://s3-eu-west-1.amazonaws.com/jamcams.tfl.gov.uk/jamcams-camera-list.xml";
                var locationsRequest = "http://content.tfl.gov.uk/camera-list.xml";
                var txt = MakeRequest(locationsRequest);
                var result = txt.Deserialize(typeof(syndicatedFeed)) as syndicatedFeed;
                ProcessResponse(result, config.LocalAreaNames, config);
            }
            catch (Exception ex)
            {
                Logger.Write($"CameraIndexer failed: " + ex.Message, "CameraIndexer");
                // ignored
            }
        }

        public static void ProcessResponse(syndicatedFeed feed, PolygonManager localAreas, BuildIndexSettings config)
        {
            if (feed == null)
                return;

            var docs = new List<LocationDocument>();

            foreach (var p in feed.cameraList.cameras)
            {
                config.RecordsCurrent++;

                var point = new PointGeoShape(new GeoCoordinate(p.lat, p.lng));
                var loc = new GeoLocation(p.lat, p.lng);

                var terms = GetLocalAreas(loc, localAreas);
                
                p.location = p.location.Replace("/", " / ");

                var description = "CAMERA " + p.location.ToUpper();
                //if (feed.cameraList.rooturl == null)
                    feed.cameraList.rooturl = "https://s3-eu-west-1.amazonaws.com/jamcams.tfl.gov.uk/";

                DateTime created;
                DateTime.TryParse(p.captureTime, out created);
                
                var address = new LocationDocument
                {
                    Created = created,
                    Type = IndexBuilder.AddressDocumentType.Camera,
                    Source = "TFL",
                    ID = IndexBuilder.AddressDocumentType.Camera + p.id,
                    //BuildingName = "",
                    indextext = Join(description, terms, false).Decompound(config.DecompoundList) + " CCTV",
                    Description = description,
                    Location = loc,
                    Point = point,
                    //Organisation = "TfL",
                    //Postcode = p.postCode,
                    //SubBuilding = "",
                    Thoroughfare = null,
                    Locality = new List<string>(),
                    Areas = terms,
                    Status = p.available == "true" ? p.currentView : "offline",
                    Image =  feed.cameraList.rooturl + p.file,
                    Video = feed.cameraList.rooturl + p.id + ".mp4"
                };

                if (Math.Abs(p.lat) < 1 && Math.Abs(p.lng) < 1)
                {
                    //IndexBuilder.DeleteItem<AddressDocument>(address);
                    config.Skipped++;
                }
                else
                    docs.Add(address);
            }

            IndexItems(docs.ToArray(), config);

            config.RecordsTotal = feed.cameraList.cameras.Count;
        }


        [Serializable]
        [KnownType(typeof(CameraList))]
        public class syndicatedFeed
        {
            public CameraList cameraList;
            public TfLCameraHeader header;
        }

        [Serializable]
        public class TfLCameraHeader
        {
            public string author;
            public string errorMessage;
            public string feedInfo;
            public string identifier;
            public string max_Latency;
            public string overrideMessage;
            public string owner;
            public string publishDateTime;
            public string refreshRate;
            public string timeToError;
            public string version;
        }

        [Serializable]
        [KnownType(typeof(Camera))]
        public class CameraList
        {
            [XmlElement(ElementName = "camera")] public List<Camera> cameras;

            public string rooturl;
        }

        [Serializable]
        [XmlRoot(ElementName = "camera", Namespace = "")]
        public class Camera
        {
            [XmlAttribute] public string available;

            public string captureTime;

            public string corridor;
            public string currentView;
            public float easting;
            public string file;

            [XmlAttribute] public string id;

            public float lat;
            public float lng;
            public string location;
            public float northing;
            public string osgr;
            public string postCode;
        }
    }
}