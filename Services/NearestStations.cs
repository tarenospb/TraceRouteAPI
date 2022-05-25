using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TraceRouteApi.Settings;
using Microsoft.Extensions.Options;
using TraceRouteApi.Models.NearestStations;

namespace TraceRouteApi.Services
{
    public class NearestStations : INearestStations
    {
        private readonly HttpClient _httpClient;
        private GlobalSettings _conf;

        public NearestStations(HttpClient httpClient, IOptions<GlobalSettings> conf)
        {
            _httpClient = httpClient;
            _conf = conf.Value;
        }

        public async Task<NearestStationsStruct> GetNearestStationsFromYandexServiceAsync(double km, double lat, double lng, string transportType, string stationType)
        {
            NearestStationsStruct nearestStations = new NearestStationsStruct();
            
                var request = new HttpRequestMessage(HttpMethod.Get,
                        "https://api.rasp.yandex.net/v3.0/nearest_stations/?apikey="
                    + _conf.ApiKey
                    + "&format=json&lat="
                    + lat.ToString().Replace(',', '.')
                    + "&lng="
                    + lng.ToString().Replace(',', '.')
                    + "&transport_types=" + transportType
                     + "&station_types=" + stationType
                     + "&distance=" + km
                + "&lang=ru_RU");
                var response = await _httpClient.SendAsync(request);
            

            if (response.IsSuccessStatusCode)
            {

                var text = await response.Content.ReadAsStringAsync();

                nearestStations = JsonConvert.DeserializeObject<NearestStationsStruct>(text);
              return nearestStations;
            }
            return nearestStations;
        }
    }
}
