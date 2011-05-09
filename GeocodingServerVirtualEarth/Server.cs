// For conditions of distribution and use, see AssemblyInfo.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Manifold.GeocodingServer;

namespace Manifold.GeocodingServer.VirtualEarth
{
	// Server for Virtual Earth
	public class Server : IServer
	{
		private class Result : IResult
		{
			private String _Address;
			public String Address
			{
				get { return _Address; }
				set { _Address = value; }
			}

			private String _City;
			public String City
			{
				get { return _City; }
				set { _City = value; }
			}

			private String _Country;
			public String Country
			{
				get { return _Country; }
				set { _Country = value; }
			}

			private String _State;
			public String State
			{
				get { return _State; }
				set { _State = value; }
			}

			private String _Zip;
			public String Zip
			{
				get { return _Zip; }
				set { _Zip = value; }
			}

			private Double _Latitude;
			public Double Latitude
			{
				get { return _Latitude; }
				set { _Latitude = value; }
			}

			private Double _Longitude;
			public Double Longitude
			{
				get { return _Longitude; }
				set { _Longitude = value; }
			}
		}

		private class Results : IResults
		{
			private List<Result> results;

			public Results()
			{
				results = new List<Result>();
			}

			public void Add(Result _result)
			{
				results.Add(_result);
			}

			public Int32 Count
			{
				get { return results.Count; }
			}

			public IResult this[Int32 _index]
			{
				get { return results[_index]; }
			}
		}

		//"http://local.live.com/search.ashx?b=";
		//"http://dev.virtualearth.net/legacyservice/search.ashx?b=";
		private const String c_strRequestUri = "http://dev.virtualearth.net/services/v1/geocodeservice/geocodeservice.asmx/Geocode?count=&landmark=&addressLine=&locality=&postalTown=&adminDistrict=&district=&postalCode=&countryRegion=&mapBounds=&currentLocation=&curLocAccuracy=&entityTypes=&rankBy=&culture=%22en-us%22&format=json&rid=111&query=";
		private const String c_strName = "Virtual Earth";

		public Server()
		{
		}

		public IResults Geocode(string _address)
		{
            Results results = new Results();
            Uri uriRequest = new Uri(c_strRequestUri + "\"" + _address + "\"");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriRequest);

            WebProxy proxy = null;
            
            IWebProxy proxySystem = HttpWebRequest.GetSystemWebProxy();
            Uri uriProxy = proxySystem.GetProxy(uriRequest);

            if (uriProxy != uriRequest)
            {
                proxy = new WebProxy(uriProxy);
                proxy.UseDefaultCredentials = true;
                request.Proxy = proxy;
            }                      

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            String content = new StreamReader(response.GetResponseStream()).ReadToEnd();

			
            Regex reg = new Regex(
                "\"Name\":\"(?<name>.*?)\".*?,\"BestLocation\":{.*?\"Latitude\":(?<lat>.*?),\"Longitude\":(?<lon>.*?)}}.*?\"Locality\":\"(?<locality>.*?)\",.*?\"AdminDistrict\":\"(?<state>.*?)\",\"PostalCode\":\"(?<code>.*?)\",\"CountryRegion\":\"(?<country>.*?)\""
                , RegexOptions.IgnoreCase);

            foreach (Match match in reg.Matches(content)) //mResult.Result("${result}"
            {
				Result r = new Result();
            
                r.Address = match.Result("${name}");
                r.Country = match.Result("${country}");
                r.City = match.Result("${locality}");
                r.State = match.Result("${state}");
                r.Zip = match.Result("${code}");
                try {
					r.Latitude = Double.Parse(match.Result("${lat}"));
					r.Longitude = Double.Parse(match.Result("${lon}"));
                } 
                catch (ArgumentNullException) { } 
                catch (FormatException) { }
                catch (OverflowException) { }

                results.Add(r);
            }
            
			/*
            Regex regex = new Regex(@"AddLocation\(.*?\)", RegexOptions.IgnoreCase);
            MatchCollection matchCollection = regex.Matches(content);

			foreach (Match match in matchCollection)
			{
				Double dLongitude;
				Double dLatitude;

				String m = match.Value.Remove(0, 12); m = m.Remove(m.Length - 1);
				if (!m.StartsWith("'")) continue;
				Int32 a = m.IndexOf("'", 1); if (a == -1) continue;
				String address = HttpUtility.HtmlDecode(m.Substring(1, a - 1));
				m = m.Substring(a + 1);
				String[] marr = m.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				try
				{
					dLongitude = Convert.ToDouble(marr[1].Trim(), CultureInfo.InvariantCulture);
					dLatitude = Convert.ToDouble(marr[0].Trim(), CultureInfo.InvariantCulture);
				}
				catch (InvalidCastException) { continue; }

				results.Add(new Result(address, dLongitude, dLatitude));
			}

			regex = new Regex(@"new Array\('.*?',.*?\)", RegexOptions.IgnoreCase);
			matchCollection = regex.Matches(content);

			foreach (Match match in matchCollection)
			{
				Double dLongitude;
				Double dLatitude;

				String m = match.Value.Remove(0, 10); m = m.Remove(m.Length - 1);
				if (!m.StartsWith("'")) continue;
				Int32 a = m.IndexOf("'", 1); if (a == -1) continue;
				String address = HttpUtility.HtmlDecode(m.Substring(1, a - 1));
				m = m.Substring(a + 1);
				String[] marr = m.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				try
				{
					dLongitude = (Convert.ToDouble(marr[1].Trim(), CultureInfo.InvariantCulture) + Convert.ToDouble(marr[3].Trim(), CultureInfo.InvariantCulture)) / 2;
					dLatitude = (Convert.ToDouble(marr[0].Trim(), CultureInfo.InvariantCulture) + Convert.ToDouble(marr[2].Trim(), CultureInfo.InvariantCulture)) / 2;
				}
				catch (InvalidCastException) { continue; }

				results.Add(new Result(address, dLongitude, dLatitude));
			}

            if (results.Count == 0)
            {
                regex = new Regex(@"SetViewport\(.*?\)", RegexOptions.IgnoreCase);
                Match match = regex.Match(content);
                if (match.Success)
                {
                    Double dLongitude;
                    Double dLatitude;

                    String m = match.Value.Remove(0, 12); m = m.Remove(m.Length - 1);
                    String[] marr = m.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    try
                    {
                        dLongitude = (Convert.ToDouble(marr[1].Trim(), CultureInfo.InvariantCulture) + Convert.ToDouble(marr[3].Trim(), CultureInfo.InvariantCulture)) / 2;
                        dLatitude = (Convert.ToDouble(marr[0].Trim(), CultureInfo.InvariantCulture) + Convert.ToDouble(marr[2].Trim(), CultureInfo.InvariantCulture)) / 2;
                        results.Add(new Result("", dLongitude, dLatitude));
                    }
                    catch (InvalidCastException) { }
                }
            }
			*/

			return results;
		}

		public String Name
		{
			get { return c_strName; }
		}

		public String Country
		{
			get { return ""; }
		}

		public Boolean Remote
		{
			get { return true; }
		}
	}
}
