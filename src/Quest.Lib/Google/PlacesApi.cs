using Newtonsoft.Json;
using Quest.Common.Messages.GIS;
using Quest.Lib.Google.Places;
using System;
using System.Collections.Generic;
using System.Net;

namespace Quest.Lib.Google
{
    public class PlacesApi
    {
        private readonly IWebClientFactory webClientFactory;
        private readonly string baseUrl = "https://maps.googleapis.com/maps/api/place/textsearch/json";
        private string key = "AIzaSyDDhZGZ8X3ajP-oqwx9g-YvB1T-dv93Ppo";

        public PlacesApi(IWebClientFactory webClientFactory)
        {
            this.webClientFactory = webClientFactory;
        }

        //https://developers.google.com/places/web-service/search#PlaceSearchResults
        //https://maps.googleapis.com/maps/api/place/textsearch/json?query=skatepark%20london&key=
        public Place Search(string text)
        {
            List<string> parms = new List<string>();

            parms.Add($"query={WebUtility.UrlEncode(text)}");
            parms.Add($"key={WebUtility.UrlEncode(key)}");

            var final = baseUrl + "?" + string.Join("&", parms);

            return Download(final);
        }

        private Place Download(string url)
        {
            using (var webClient = webClientFactory.Create())
            {
                var json = webClient.DownloadString(url);
                return JsonConvert.DeserializeObject<Place>(json);
            }
        }
    }
}
