using Newtonsoft.Json;
using Quest.Common.Messages.GIS;
using Quest.Lib.Coords;
using Quest.Lib.Google.DistanceMatrix;
using System;
using System.Collections.Generic;

namespace Quest.Lib.Google
{
    public class DistanceApi
    {
        private readonly IWebClientFactory webClientFactory;
        private readonly string baseUrl;

        public DistanceApi(string baseUrl, IWebClientFactory webClientFactory)
        {
            this.webClientFactory = webClientFactory;

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentNullException("baseUrl");

            this.baseUrl = baseUrl;
        }

        public Result Calculate(LatLng from, LatLng to, DateTime? departure_time = null, string language =null, string region=null, string mode = null, string key = null)
        {
            List<string> parms = new List<string>();

            parms.Add($"origins={from.Latitude},{from.Longitude}");
            parms.Add($"destinations={to.Latitude},{to.Longitude}");

            if (mode != null)
                parms.Add($"mode={mode}");

            if (key != null)
                parms.Add($"key={key}");

            if (language != null)
                parms.Add($"language={key}");

            if (departure_time != null)
            {
                var newunixtime = (int)departure_time.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                parms.Add($"departure_time={newunixtime}");
            }

            var final = baseUrl + "?" + string.Join("&", parms);

            return Download(final);
        }


        private Result Download(string url)
        {
            using (var webClient = webClientFactory.Create())
            {
                var json = webClient.DownloadString(url);
                return JsonConvert.DeserializeObject<Result>(json);
            }
        }
    }
}
