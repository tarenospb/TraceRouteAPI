namespace TraceRouteApi.Models.SheduleOnStation
{
    public class Schedule
    {
        public string arrival { get; set; } = null;
        public Thread thread { get; set; }
        public string departure { get; set; } = null;
        public string terminal { get; set; } = null;
        public string platform { get; set; } = null;


    }
}
