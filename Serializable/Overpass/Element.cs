using System.Collections.Generic;
namespace PolygonParser.Serializable.Overpass
{
	public class Element
	{
		public Element()
		{

		}

		public string Type { get; set; }
		public Dictionary<string, string> Tags { get; set; }

	}
}
