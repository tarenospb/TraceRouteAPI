using System;
using System.Collections.Generic;

namespace TraceRouteApi.Models.StationsList
{
    public class Stations
    {
        public string direction { get; set; }
        public Codes codes { get; set; }
        public string station_type { get; set; }
        public string title { get; set; }
        public double? longitude { get; set; }
        public string transport_type { get; set; }
        public double? latitude { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Stations stations &&
                   EqualityComparer<Codes>.Default.Equals(codes, stations.codes) &&
                   station_type == stations.station_type &&
                   title == stations.title &&
                   longitude == stations.longitude &&
                   transport_type == stations.transport_type &&
                   latitude == stations.latitude;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(codes, station_type, title, longitude, transport_type, latitude);
        }
    }
}
