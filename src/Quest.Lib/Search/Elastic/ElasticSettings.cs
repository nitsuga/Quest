using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Elasticsearch.Net;
using Nest;
using Quest.Lib.Trace;

namespace Quest.Lib.Search.Elastic
{

    public class ElasticClientFactory
    {

        public static ElasticClient CreateClient(ElasticSettings settings)
        {
            var env = Environment.GetEnvironmentVariable("ElasticUrls");
            if (env != null)
            {
                Logger.Write($"Overriding ElasticUrls address with {env}");
                settings.ElasticUrls = env;
            }

            var pool = new StaticConnectionPool(settings.Urls);
            var connsettings = new ConnectionSettings(pool);

            var user = Environment.GetEnvironmentVariable("ElasticUser");
            if (user != null)
            {
                Logger.Write($"Overriding Elastic User with {user}");
                settings.User = user;
            }

            var pass = Environment.GetEnvironmentVariable("ElasticPwd");
            if (pass != null)
            {
                Logger.Write($"Overriding Elastic Password with {pass}");
                settings.Password = pass;
            }

            if (settings.User != null)
            {
                connsettings.BasicAuthentication(settings.User, settings.Password);
            }

            connsettings.DefaultIndex(settings.DefaultIndex);

            if (settings.Debug)
                AddDebug(connsettings);

            // end of request viewing bit
            ElasticClient client = new ElasticClient(connsettings);

            return client;
        }

        static void AddDebug(ConnectionSettings settings)
        {
            // added this code in to view the raw JSON of the request
            settings.DisableDirectStreaming();
            settings.OnRequestCompleted(response =>
            {
                // log out the request and the request body, if one exists for the type of request
                if (response.RequestBodyInBytes != null)
                {
                    Debug.Print(
                        $"{response.HttpMethod} {response.Uri} \n" +
                        $"{Encoding.UTF8.GetString(response.RequestBodyInBytes)}");
                }
                else
                {
                    Debug.Print($"{response.HttpMethod} {response.Uri}");
                }

            });

        }
    }

    public class ElasticSettings
    {
        public const string DefaultDocindex = "locations";
        public const string GeofenceIndex = "geofence";
        public const string Overlayindex = "overlays";
        public const string Historyindex = "history";

        public string IndexGroups { get; set; }
        public Uri[] Urls;
        public string User { get; set; }
        public string ElasticUrls {
            set
            {
                Urls = value.Split(',').Select(x => new Uri(x)).ToArray();
            }
        }        
        public string Password { get; set; }
        public string DefaultIndex { get; set; }
        public string SynonymsFile { get; set; }
        public string LocalAreasFile { get; set; }
        public string MasterAreaFile { get; set; }

        public bool Debug { get; set; }

        public ElasticSettings()
        {
        }
   
    }
}