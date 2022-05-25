using System.Collections.Generic;

namespace TraceRouteApi.Models.SheduleOnStation
{
    public class SheduleOnStationStrust
    {
        public string date { get; set; }
        public List<Schedule> schedule { get; set; }

        public CurrentStation station { get; set; }

    }
}
