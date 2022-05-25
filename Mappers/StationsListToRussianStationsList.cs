using TraceRouteApi.Models.StationsList;
using System.Collections.Generic;

namespace TraceRouteApi.Mappers
{
    public class StationsListToRussianStationsList
    {
        private StationsListStruct _basicStationsList;
        public StationsListToRussianStationsList(StationsListStruct basicStationsList)
        {
            _basicStationsList = basicStationsList;
        }

        public List<RussianStationsListStruct> ToRussianStationsList()
        {
            var newList = new List<RussianStationsListStruct>();
            foreach (var country in _basicStationsList.countries)
            {
                if (country.title == "Россия")
                {
                    foreach (var region in country.regions)
                    {
                        foreach (var settlement in region.settlements)
                        {
                            foreach (var station in settlement.stations)
                            {
                                var newStation = new RussianStationsListStruct();
                                newStation.regionTitle = region.title;
                                newStation.settlementTitle = settlement.title;
                                newStation.stationCode = station.codes.yandex_code;
                                newStation.stationName = station.title;
                                newStation.transportType = station.transport_type;
                                if (station.longitude != null) newStation.stationLongitude = (double)station.longitude;
                                if (station.latitude != null) newStation.stationLatitude = (double)station.latitude;
                                
                                newList.Add(newStation);
                            }
                        }
                    }
                }
            }
            return newList;
        }
    }
}
