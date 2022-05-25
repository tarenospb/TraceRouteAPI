using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TraceRouteApi.Settings;
using Microsoft.Extensions.Options;
using TraceRouteApi.Models.ListStationsRoute;
using TraceRouteApi.Mappers;

namespace TraceRouteApi.Services
{
    public class ListStationsRoute : IListStationsRoute
    {
        private readonly HttpClient _httpClient;
        private GlobalSettings _conf;

        public ListStationsRoute(HttpClient httpClient, IOptions<GlobalSettings> conf)
        {
            _httpClient = httpClient;
            _conf = conf.Value;
        }

        public async Task<ListStationsRouteStruct> GetListStationsRouteFromYandexServiceAsync(string uid, DateTime date)
        {
            ListStationsRouteStruct listStationsRoute = new ListStationsRouteStruct();
            DateConvert dc = new DateConvert(date);
            
                var request = new HttpRequestMessage(HttpMethod.Get,
                        "https://api.rasp.yandex.net/v3.0/thread/?apikey="
                    + _conf.ApiKey
                    + "&lang=ru_RU&show_systems=all&date="
                    + dc.ConvertFromDateToShortString()
                    + "&uid="
                    + uid);
                var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {

                var text = await response.Content.ReadAsStringAsync();

                listStationsRoute = JsonConvert.DeserializeObject<ListStationsRouteStruct>(text);
              return listStationsRoute;
            }
            return listStationsRoute;
        }
    }
}
