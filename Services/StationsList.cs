using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TraceRouteApi.Settings;
using Microsoft.Extensions.Options;
using TraceRouteApi.Models.StationsList;

namespace TraceRouteApi.Services
{
    public class StationsList : IStationsList
    {
        private readonly HttpClient _httpClient;
        private GlobalSettings _conf;

        public StationsList(HttpClient httpClient, IOptions<GlobalSettings> conf)
        {
            _httpClient = httpClient;
            _conf = conf.Value;
        }

        public async Task<StationsListStruct> GetStationsListFromYandexServiceAsync()
        {
            var stationsList = new StationsListStruct();
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api.rasp.yandex.net/v3.0/stations_list/?apikey="
            + _conf.ApiKey
            + "&lang=ru_RU&format=json");
              var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {

                var text = await response.Content.ReadAsStringAsync();

                stationsList = JsonConvert.DeserializeObject<StationsListStruct>(text);
              return stationsList;
            }
            return stationsList;
        }
    }
}
