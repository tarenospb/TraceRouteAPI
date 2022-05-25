using System.Collections.Generic;

namespace TraceRouteApi.Models.ListStationsRoute
{
    public class ListStationsRouteStruct
    {
        public string from { get; set; }
        public string to { get; set; }
        public string transport_type { get; set; }
        public List<Stops> stops { get; set; }
        public string vehicle { get; set; }

    }
}
