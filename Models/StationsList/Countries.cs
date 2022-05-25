using System;
using System.Collections.Generic;

namespace TraceRouteApi.Models.StationsList
{
    public class Countries
    {
        public List<Regions> regions { get; set; }
        public string title { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Countries countries &&
                   EqualityComparer<List<Regions>>.Default.Equals(regions, countries.regions) &&
                   title == countries.title;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(regions, title);
        }
    }
}
