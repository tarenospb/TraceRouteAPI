using System;

namespace TraceRouteApi.Models.StationsList
{
    public class RussianStationsListStruct
    {
        public string regionTitle { get; set; }
        public string settlementTitle { get; set; }
        public string stationCode { get; set; }
        public string stationName { get; set; }
        public string transportType { get; set; }
        public double stationLongitude { get; set; }
        public double stationLatitude { get; set; }

        public RussianStationsListStruct()
        {

        }

        public override bool Equals(object obj)
        {
            return obj is RussianStationsListStruct @struct &&
                   regionTitle == @struct.regionTitle &&
                   settlementTitle == @struct.settlementTitle &&
                   stationCode == @struct.stationCode &&
                   stationName == @struct.stationName &&
                   transportType == @struct.transportType &&
                   stationLongitude == @struct.stationLongitude &&
                   stationLatitude == @struct.stationLatitude;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(regionTitle, settlementTitle, stationCode, stationName, transportType, stationLongitude, stationLatitude);
        }
    }
}
