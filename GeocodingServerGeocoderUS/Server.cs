// For conditions of distribution and use, see AssemblyInfo.cs

using System;
using System.Net;
using Manifold.GeocodingServer;
using System.IO;
using System.Collections.Generic;

namespace Manifold.GeocodingServer.GeocoderUS
{
    // Server for geocoder.us web site
    public class Server : IServer
    {
        // Create new server
        public Server()
        {
        }

        // Geocode address
        public IResults Geocode(String _address)
        {
            // todo:
            // http://geocoder.us/help/
            // http://rpc.geocoder.us/service/namedcsv?address=1600+Pennsylvania+Ave,+Washington+DC&parse_address=1

            Results results = new Results();

            try
            {
                String url = "http://rpc.geocoder.us/service/csv?address=" + _address;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                WebProxy myProxy = new WebProxy();
                myProxy.UseDefaultCredentials = true;
                request.Proxy = myProxy;

                // obtain response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());

                String strResults = reader.ReadToEnd();
                String[] addresses = strResults.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String addr in addresses)
                {
                    String[] values = addr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length > 2)
                    {
                        Double lat = 0; Double.TryParse(values[0], out lat);
                        Double lon = 0; Double.TryParse(values[1], out lon);
                        String address = addr.Substring(values[0].Length + values[1].Length + 2);
                        results.Add(new Result(lat, lon, address));
                    }
                }
            }
            catch (ArgumentException) { }
            catch (WebException) { }

            return results;
        }

        // Get country (blank if server supports more than one country)
        public String Country
        {
            get { return GetCountry(); }
        }

        // Get name
        public String Name
        {
            get { return "Geocoder.us"; }
        }

        // Check if server is remote (expensive to connect to)
        public Boolean Remote
        {
            get { return true; }
        }

        internal static string GetCountry()
        {
            return "United States";
        }
   }

    // Geocoding result interface
    public class Result : IResult
    {
        private String address;
        private String city;
        private String country;
        private Double latitude;
        private Double longitude;
        private String state;
        private String zip;

        public Result(Double lat, Double lon, String addr)
        {
            latitude = lat;
            longitude = lon;
            address = addr;
            city = "";
            country = "";
            state = "";
            zip = "";
        }

        // Get street address
        public String Address 
        {
            get { return address; } 
        }

        // Get city
        public String City
        {
            get { return city; }
        }

        // Get country
        public String Country
        {
            get { return country; }
        }

        // Get latitude (InvalidCoord if unknown)
        public Double Latitude
        {
            get { return latitude; }
        }

        // Get longitude (InvalidCoord if unknown)
        public Double Longitude
        {
            get { return longitude; }
        }

        // Get state
        public String State
        {
            get { return state; }
        }

        // Get zip code
        public String Zip
        {
            get { return zip; }
        }
    }

    // Geocoding results interface
    public class Results : IResults
    {
        private List<Result> results = new List<Result>();

        public void Add(Result result)
        {
            results.Add(result);
        }

        // Get number of results
        public Int32 Count { get { return results.Count; } }

        // Get result with given index
        public IResult this[Int32 _index] { get { return results[_index];  } }
    }
}
