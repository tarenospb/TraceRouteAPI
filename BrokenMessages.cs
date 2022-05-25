namespace TraceRouteApi
{
    public class BrokenMessages
    {
        public readonly string NearestStationsInoperativeMsg 
            = "NearestStations Service is inoperative, please try later on";

        public readonly string StationsListInoperativeMsg
            = "StationsList Service is inoperative, please try later on";
        public readonly string SheduleOnStationInoperativeMsg
            = "SheduleOnStation Service is inoperative, please try later on";
        public readonly string PointPointInoperativeMsg
            = "ShedulePointPoint Service is inoperative, please try later on";
        public readonly string ListStationsRouteInoperativeMsg
            = "ListStationsRoute Service is inoperative, please try later on";

        public readonly string WrongCityMsg
            = "Point must be in Russian Federation";
        public readonly string WrongDateMsg
            = "Date must be in format DD.MM.YYYY";
        public readonly string WrongRadMsg
            = "Radius must be in interval 0-20 kms";
        public readonly string WrongTransportTypeMsg
            = "Transport type must be [bus,train,plane] or all value";
    }
}
