using System.Collections.Generic;

namespace TraceRouteApi.Models.ShedulePointPoint
{
    public class Segments
    {
        public string arrival { get; set; }
        public FromTo from { get; set; }
        public Thread thread { get; set; }
        public string departure { get; set; }
        public FromTo to { get; set; }
        public double duration { get; set; }
        public string start_date { get; set; }
        public Tickets_info tickets_info { get; set; }

    }
}
