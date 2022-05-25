using System.Threading.Tasks;
using System;
using TraceRouteApi.Models.ListStationsRoute;

namespace TraceRouteApi.Services
{
    public interface IListStationsRoute
    {
        Task<ListStationsRouteStruct> GetListStationsRouteFromYandexServiceAsync(string uid, DateTime date);
    }
}
