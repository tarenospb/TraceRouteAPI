using System.Threading.Tasks;
using TraceRouteApi.Models.StationsList;

namespace TraceRouteApi.Services
{
    public interface IStationsList
    {
        Task<StationsListStruct> GetStationsListFromYandexServiceAsync();
    }
}
