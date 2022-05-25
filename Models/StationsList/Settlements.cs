using System;
using System.Collections.Generic;

namespace TraceRouteApi.Models.StationsList
{
    public class Settlements
    {
        public string title { get; set; }
        public List<Stations> stations { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Settlements settlements &&
                   title == settlements.title &&
                   EqualityComparer<List<Stations>>.Default.Equals(stations, settlements.stations);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(title, stations);
        }
    }
}
