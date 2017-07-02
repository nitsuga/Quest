using System.Collections.Generic;
using System.Web;

namespace Quest.Mobile.Code
{
    public class LocationInfo
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string Name { get; set; }
    }

    public class GeoLocationService
    {
        public static LocationInfo GetLocationInfo()
        {
            //TODO: How/where do we refactor this and tidy up the use of Context? This isn't testable.
            string ipaddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            var v = new LocationInfo();

            if (ipaddress != "127.0.0.1")
                v = GetLocationInfo(ipaddress);
            else //debug locally
                v = new LocationInfo()
                {
                    Name = "Sugar Grove, IL",
                    CountryCode = "US",
                    CountryName = "UNITED STATES",
                    Latitude = 41.7696F,
                    Longitude = -88.4588F
                };
            return v;
        }

        private static Dictionary<string, LocationInfo> cachedIps = new Dictionary<string, LocationInfo>();

        public static LocationInfo GetLocationInfo(string ipParam)
        {
            if (ipParam=="::1")
            {
                ipParam = "86.29.75.151";
            }

            var i = System.Net.IPAddress.Parse(ipParam);
            var ip = i.ToString();

#if false
            LocationInfo result = null;
            if (!cachedIps.ContainsKey(ip))
            {
                string r;
                using (var w = new WebClient())
                {
                    r = w.DownloadString(String.Format("http://api.hostip.info/?ip={0}&position=true", ip));
                }

                var xmlResponse = XDocument.Parse(r);
                var gml = (XNamespace)"http://www.opengis.net/gml";
                var ns = (XNamespace)"http://www.hostip.info/api";

                try
                {
                    result = (from x in xmlResponse.Descendants(ns + "Hostip")
                              select new LocationInfo
                              {
                                  CountryCode = x.Element(ns + "countryAbbrev").Value,
                                  CountryName = x.Element(ns + "countryName").Value,
                                  Latitude = float.Parse(x.Descendants(gml + "coordinates").Single().Value.Split(',')[0]),
                                  Longitude = float.Parse(x.Descendants(gml + "coordinates").Single().Value.Split(',')[1]),
                                  Name = x.Element(gml + "name").Value
                              }).SingleOrDefault();
                }
                catch (NullReferenceException)
                {
                    //Looks like we didn't get what we expected.
                }
                if (result != null)
                {
                    cachedIps.Add(ip, result);
                }
        }
            else
            {
                result = cachedIps[ip];
            }
#else
            var result = new LocationInfo { Latitude = 51.1f, Longitude = -0.1f };
#endif
            return result;
        }
    }
}