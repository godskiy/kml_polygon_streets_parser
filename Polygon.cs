using System;
using System.Collections.Generic;

namespace PolygonParser
{
	public class Polygon
	{
		public Polygon(Guid guid, string description, List<Vertice> vertices)
		{
			Guid = guid;
			Description = description;
			Vertices = vertices;
		}

		public Guid Guid { get; set; }

		public List<Vertice> Vertices { get; set; }

		public string Description { get; set; }
	}

	public class Vertice
	{
		public Vertice(double longitude, double latitude)
		{
			Longitude = longitude;
			Latitude = latitude;
		}
		public double Longitude { get; set; }
		public double Latitude { get; set; }
	}
}
