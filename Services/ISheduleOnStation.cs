using System.Threading.Tasks;
using System;
using TraceRouteApi.Models.SheduleOnStation;

namespace TraceRouteApi.Services
{
    public interface ISheduleOnStation
    {
        Task<SheduleOnStationStrust> GetSheduleOnStationFromYandexServiceAsync(string station, DateTime date, string transportType);
    }
}
