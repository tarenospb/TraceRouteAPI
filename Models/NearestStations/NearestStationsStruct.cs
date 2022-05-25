using System;
using System.Collections.Generic;

namespace TraceRouteApi.Models.NearestStations
{
    public class NearestStationsStruct
    {
        public Pagination pagination { get; set; }
        public List<Stations> stations { get; set; }

        public override bool Equals(object obj)
        {
            return obj is NearestStationsStruct @struct &&
                   EqualityComparer<Pagination>.Default.Equals(pagination, @struct.pagination) &&
                   EqualityComparer<List<Stations>>.Default.Equals(stations, @struct.stations);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(pagination, stations);
        }
    }
}
