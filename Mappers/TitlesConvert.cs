using System.Collections.Generic;

namespace TraceRouteApi.Mappers
{
    public class TitlesConvert
    {
        private string _title;

        public TitlesConvert(string title)
        {
            _title = title;
        }

        /// <summary>
        /// получение второго города в отправлении
        /// </summary>
        public string GetSecondPointInDeparture()
        {
            var needString = "";
            int j = 0;
            foreach (var i in _title)
            {
                if (i == '\u2014')
                {
                    for (int k = j+1; k < _title.Length; k++)
                    {
                        var ii = _title[k];
                        if (ii != ' ')
                        {
                            if (ii == '('
                                || ii == ')'
                                || ii == ',') break;
                            needString += ii;
                        }
                    }
                }
                j++;
            }
            return needString;
        }

        /// <summary>
        /// создание типов транспорта на основании запроса юзера
        /// </summary>
        public List<string> GetTransportTypeList()
        {
            var list = new List<string>();
            var types = _title.Split(',');
            string[] titles = {"plane","train", "bus" };
            for (int j = 0; j < 3; j++)
            {
                foreach (var i in types)
                {
                    if (i == titles[j])
                    {
                        list.Add(i);
                    }
                }
            }
            if (list.Count == 0)
            {
                list.AddRange(titles);
            }
            

            return list;
        }

        /// <summary>
        /// создание типов станций в зависимости от типов транспорта
        /// </summary>
        public List<string> CreateStationTypes(List<string> transportTypes)
        {
            var list = new List<string>();
            foreach (var i in transportTypes)
            {
                if (i == "bus")
                {
                    list.Add("bus_station,station");
                }
                if (i == "plane")
                {
                    list.Add("airport");
                }
                if (i == "train")
                {
                    list.Add("train_station,station");
                }
            }

            return list;
        }
    }
}
