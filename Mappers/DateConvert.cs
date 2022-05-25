using System;

namespace TraceRouteApi.Mappers
{
    public class DateConvert
    {
        private string _needStringDate;
        private DateTime _needDate;
        public DateConvert(string date)
        {
            _needDate = ConvertToNeedDate(date);
        }
        public DateConvert(DateTime date)
        {
            _needStringDate = ConvertToNeedDate(date);
        }

        /// <summary>
        /// конвертирует дату в строку YYYY-MM-DD
        /// </summary>
        private string ConvertToNeedDate(DateTime date)
        {
            string mm = date.Month.ToString();
            string dd = date.Day.ToString();
            if (date.Month < 10)
            {
                mm = "0" + date.Month.ToString();
            }
            if (date.Day < 10)
            {
                mm = "0" + date.Day.ToString();
            }
            return (date.Year + "-" + mm + "-" + dd);
        }

        /// <summary>
        /// конвертирует строку типа YYYY-MM-DDThh-mm-ss в дату
        /// </summary>
        private DateTime ConvertToNeedDate(string date)
        {
            var year = Convert.ToInt32(date.Substring(0,4));
            var mm = Convert.ToInt32(date.Substring(5, 2));
            var dd = Convert.ToInt32(date.Substring(8, 2));
            var hh = Convert.ToInt32(date.Substring(11, 2));
            var min = Convert.ToInt32(date.Substring(14, 2));
            var ss = Convert.ToInt32(date.Substring(17, 2));
            var d = new DateTime(year,mm,dd,hh,min,ss);
            return d;
        }

        public string ConvertFromDateToShortString()
        {
            return _needStringDate;
        }
        public DateTime ConvertFromLongStringDateToDate()
        {
            return _needDate;
        }
    }
}
