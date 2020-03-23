using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
namespace PolygonParser
{
	class Program
	{
		readonly static OverpassApiClient _apiClient = new OverpassApiClient("https://overpass-api.de/api/interpreter");
		const string _resultFileName = "StreetsByPolygon.json";
		private static List<Polygon> GetPolygons(string kmlFilePath, string KmlNamepsace)
		{
			dynamic placemarks;

			using (FileStream stream = new FileStream(kmlFilePath, FileMode.Open))
			{
				XDocument xDocument = XDocument.Load(stream);
				XNamespace ns = KmlNamepsace;
				placemarks = xDocument
					.Descendants(ns + "Placemark")
					.Select(x => new { Description = x.Element(ns + "description").Value, Coordinates = x.Element(ns + "Polygon").Value })
					.ToList();
			}

			if (placemarks.Count == 0)
			{
				throw new ArgumentException("No placemarks provided in kml file");
			}

			List<Polygon> polygons = new List<Polygon>();

			foreach (dynamic placemark in placemarks)
			{
				string[] stringCoordinates = placemark.Coordinates.Split(' ');

				List<Vertice> vertices = new List<Vertice>(stringCoordinates.Length);

				foreach (string verticeCoordinates in stringCoordinates)
				{
					string[] longitudeAndLatitude = verticeCoordinates.Split(',');
					Vertice vertice = new Vertice(double.Parse(longitudeAndLatitude[1]), double.Parse(longitudeAndLatitude[0]));
					vertices.Add(vertice);
				}

				polygons.Add(new Polygon(Guid.NewGuid(), placemark.Description, vertices));
			}

			return polygons;
		}

		static void Main(string[] arguments)
		{
			if(arguments.Length != 2 || !File.Exists(arguments[0]))
			{
				Console.Error.WriteLine("You must provide:");
				Console.Error.WriteLine("1. Absolute path of kml file");
				Console.Error.WriteLine("2. kml namespace");
				Console.Error.WriteLine("Example: PolygonParser.exe yourabsolutepath\\map.kml http://www.opengis.net/kml/2.2");
				Console.ReadLine();
				return;
			}

			string mapPath = arguments[0];
			string nameSpace = arguments[1];

			try
			{
				List<Polygon> polygons = GetPolygons(mapPath, nameSpace);

				Dictionary<string, List<string>> streetsByPolygon = new Dictionary<string, List<string>>();

				foreach(Polygon polygon in polygons)
				{
					var coordinates = "";

					foreach (Vertice vertice in polygon.Vertices)
					{
						if (string.Empty != coordinates)
						{
							coordinates = coordinates + " " + vertice.Longitude + " " + vertice.Latitude;
						}
						else
						{
							coordinates = vertice.Longitude + " " + vertice.Latitude;
						}
					}

					var answer = _apiClient.QueryPolygon(coordinates).Result;

					List<string> streets = new List<string>();

					foreach(var element in answer.Elements)
					{
						if(element.Type != "way")
						{
							continue;
						}

						string name = element.Tags["name"];

						if (!streets.Contains(name))
						{
							streets.Add(name);
						}
					}

					streetsByPolygon.Add(polygon.Description + "#" + polygon.Guid, streets);
				}

				string resultJson = JsonConvert.SerializeObject(streetsByPolygon);
				string filePath = Environment.CurrentDirectory + "\\" + _resultFileName;
				using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
				{
					byte[] bytes = System.Text.Encoding.Default.GetBytes(resultJson);
					stream.Write(bytes, 0, bytes.Length);
				}

				Console.WriteLine($"Streets successfuly taken. Output file path: {filePath}");
			}
			catch(Exception e)
			{
				Console.WriteLine($"Error during streets convertation: {e.ToString()}");
			}
			Console.ReadLine();
		}
	}
}
