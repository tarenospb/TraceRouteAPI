using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TraceRouteApi.Settings;
using Microsoft.Extensions.Options;
using TraceRouteApi.Models.SheduleOnStation;
using TraceRouteApi.Mappers;

namespace TraceRouteApi.Services
{
    public class SheduleOnStation : ISheduleOnStation
    {
        private readonly HttpClient _httpClient;
        private GlobalSettings _conf;

        public SheduleOnStation(HttpClient httpClient, IOptions<GlobalSettings> conf)
        {
            _httpClient = httpClient;
            _conf = conf.Value;
        }

        public async Task<SheduleOnStationStrust> GetSheduleOnStationFromYandexServiceAsync(string station, DateTime date, string transportType)
        {
            SheduleOnStationStrust sheduleOnStation = new SheduleOnStationStrust();
            HttpResponseMessage response = new HttpResponseMessage();
            DateConvert dc = new DateConvert(date);
            if (transportType == "all")
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                        "https://api.rasp.yandex.net/v3.0/schedule/?apikey="
                    + _conf.ApiKey
                    + "&event=departure&station="
                    + station
                    + "&date=" + dc.ConvertFromDateToShortString());
                response = await _httpClient.SendAsync(request);
            }
            else
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                        "https://api.rasp.yandex.net/v3.0/schedule/?apikey="
                    + _conf.ApiKey
                    + "&event=departure&station="
                    + station
                    + "&transport_types="
                    + transportType
                    + "&date=" + dc.ConvertFromDateToShortString());
                response = await _httpClient.SendAsync(request);
            }

            if (response.IsSuccessStatusCode)
            {

                var text = await response.Content.ReadAsStringAsync();

                sheduleOnStation = JsonConvert.DeserializeObject<SheduleOnStationStrust>(text);
              return sheduleOnStation;
            }
            return sheduleOnStation;
        }
    }
}
