using System;

namespace TraceRouteApi.Models.NearestStations
{
    public class Stations
    {
        public double distance { get; set; }
        public string code { get; set; } = null;
        public string station_type { get; set; } = null;
        public string station_type_name { get; set; } = null;
        public string title { get; set; } = null;
        public string popular_title { get; set; } = null;
        public string short_title { get; set; } = null;
        public string majority { get; set; } = null;
        public string transport_type { get; set; } = null;
        public double? lat { get; set; }
        public double? lng { get; set; }
        public string type { get; set; } = null;

        public override bool Equals(object obj)
        {
            return obj is Stations nSS &&
                   distance == nSS.distance &&
                   code == nSS.code &&
                   station_type == nSS.station_type &&
                   station_type_name == nSS.station_type_name &&
                   title == nSS.title &&
                   popular_title == nSS.popular_title &&
                   short_title == nSS.short_title &&
                   majority == nSS.majority &&
                   transport_type == nSS.transport_type &&
                   lat == nSS.lat &&
                   lng == nSS.lng &&
                   type == nSS.type;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(distance);
            hash.Add(code);
            hash.Add(station_type);
            hash.Add(station_type_name);
            hash.Add(title);
            hash.Add(popular_title);
            hash.Add(short_title);
            hash.Add(majority);
            hash.Add(transport_type);
            hash.Add(lat);
            hash.Add(lng);
            hash.Add(type);
            return hash.ToHashCode();
        }
    }
}
