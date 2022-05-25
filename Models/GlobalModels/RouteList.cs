using System.Collections.Generic;

namespace TraceRouteApi.Models
{
    public class RouteList
    {
        public ShedulePointPoint.Segments route { get; set; }
        public List<ListStationsRoute.Stops> stops { get; set; }
    }
}
