using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TraceRouteApi.Settings;
using Microsoft.Extensions.Options;
using TraceRouteApi.Models.ShedulePointPoint;
using TraceRouteApi.Mappers;

namespace TraceRouteApi.Services
{
    public class ShedulePointPoint : IShedulePointPoint
    {
        private readonly HttpClient _httpClient;
        private GlobalSettings _conf;

        public ShedulePointPoint(HttpClient httpClient, IOptions<GlobalSettings> conf)
        {
            _httpClient = httpClient;
            _conf = conf.Value;
        }

        public async Task<ShedulePointPointStruct> GetShedulePointPointFromYandexServiceAsync(string from, string to, DateTime date, string transportType)
        {
            ShedulePointPointStruct pointPoint = new ShedulePointPointStruct();
            HttpRequestMessage request = new HttpRequestMessage();
            DateConvert dc = new DateConvert(date);
            if (transportType == "all")
            {
                request = new HttpRequestMessage(HttpMethod.Get,
                        "https://api.rasp.yandex.net/v3.0/search/?apikey="
                    + _conf.ApiKey
                    + "&format=json&lang=ru_RU&from="
                    + from
                    + "&to="
                    + to
                     + "&date=" + dc.ConvertFromDateToShortString());
            }
            else
            {
                request = new HttpRequestMessage(HttpMethod.Get,
                        "https://api.rasp.yandex.net/v3.0/search/?apikey="
                    + _conf.ApiKey
                    + "&format=json&lang=ru_RU&from="
                    + from
                    + "&to="
                    + to
                     + "&date=" + dc.ConvertFromDateToShortString()
                    + "&transport_types=" + transportType);
                
            }

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

                var text = await response.Content.ReadAsStringAsync();

                pointPoint = JsonConvert.DeserializeObject<ShedulePointPointStruct>(text);
              return pointPoint;
            }
            return pointPoint;
        }
    }
}
