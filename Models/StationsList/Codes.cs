using System;

namespace TraceRouteApi.Models.StationsList
{
    public class Codes
    {
        public string esr_code { get; set; }
        public string yandex_code { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Codes codes &&
                   esr_code == codes.esr_code &&
                   yandex_code == codes.yandex_code;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(esr_code, yandex_code);
        }
    }
}
