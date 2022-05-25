using System;
using System.Threading.Tasks;
using TraceRouteApi.Models.ShedulePointPoint;

namespace TraceRouteApi.Services
{
    public interface IShedulePointPoint
    {
        Task<ShedulePointPointStruct> GetShedulePointPointFromYandexServiceAsync(string from, string to, DateTime date, string transportType);
    }
}
