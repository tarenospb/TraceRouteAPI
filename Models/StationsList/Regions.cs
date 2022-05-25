using System;
using System.Collections.Generic;

namespace TraceRouteApi.Models.StationsList
{
    public class Regions
    {
        public List<Settlements> settlements { get; set; }
        public string title { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Regions regions &&
                   EqualityComparer<List<Settlements>>.Default.Equals(settlements, regions.settlements) &&
                   title == regions.title;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(settlements, title);
        }
    }
}
