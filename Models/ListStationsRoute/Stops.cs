namespace TraceRouteApi.Models.ListStationsRoute
{
    public class Stops
    {
        public string arrival { get; set; }
        public string departure { get; set; }
        public double duration { get; set; }
        public double? stop_time { get; set; }
        public Station station { get; set; }
        public string terminal { get; set; }
        public string platform { get; set; }

    }
}
