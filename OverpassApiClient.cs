using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PolygonParser
{
	public class OverpassApiClient
	{
		readonly string _targetDomain;
		readonly HttpClient _httpClient = new HttpClient();
		readonly Dictionary<string, string> _parameters = new Dictionary<string, string>
		{
			{ "data", "" }
		};
		const string _overpassQueryTemplate = "[out:json];(way[\"highway\"][\"name\"](poly:\"{coordinates}\");<;); out;";
		public OverpassApiClient(string targetDomain)
		{
			_targetDomain = targetDomain;
		}

		public async Task<Serializable.Overpass.Answer> QueryPolygon(string polygonCoordinates)
		{
			_parameters["data"] = _overpassQueryTemplate.Replace("{coordinates}", polygonCoordinates);
			FormUrlEncodedContent content = new FormUrlEncodedContent(_parameters);
			//HttpResponseMessage httpResponse = await _httpClient.PostAsync("https://overpass-api.de/api/interpreter", content);
			HttpResponseMessage httpResponse = await _httpClient.PostAsync(_targetDomain, content);
			string responseString = await httpResponse.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<Serializable.Overpass.Answer>(responseString);
		}
	}
}
