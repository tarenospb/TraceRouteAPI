using System.Threading.Tasks;
using TraceRouteApi.Models.NearestStations;

namespace TraceRouteApi.Services
{
    public interface INearestStations
    {
        Task<Models.NearestStations.NearestStationsStruct> GetNearestStationsFromYandexServiceAsync(double km, double lat, double lng, string transportType, string stationType);
    }
}
