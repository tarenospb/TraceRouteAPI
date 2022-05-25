using System;

namespace TraceRouteApi.Models.NearestStations
{
    public class Pagination
    {
        public int total { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Pagination nSS &&
                   total == nSS.total &&
                   limit == nSS.limit &&
                   offset == nSS.offset;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(total, limit, offset);
        }
    }
}
