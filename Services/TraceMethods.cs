using System;
using System.Collections.Generic;
using TraceRouteApi.Services;
using TraceRouteApi.Models.NearestStations;
using TraceRouteApi.Models.SheduleOnStation;
using TraceRouteApi.Models.ShedulePointPoint;
using TraceRouteApi.Models.ListStationsRoute;
using TraceRouteApi.Models.GlobalModels;
using Microsoft.Extensions.Caching.Memory;
using TraceRouteApi.Repositories;
using System.Threading.Tasks;
using TraceRouteApi.Mappers;
using Polly.CircuitBreaker;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using TraceRouteApi.Settings;
using TraceRouteApi.Models;

namespace TraceRouteApi.Services
{
    public class TraceMethods
    {
        private CoordinateDistance _coords;
        private LatLngStruct _from, _to;
        private List<string> _transportTypes;
        private List<string> _stationTypes;
        private ListStationsRouteStruct _lastRouteStationsList;
        private List<RouteList> _route;
        private BrokenMessages _messages;
        private INearestStations _nearestStations;
        private ISheduleOnStation _sheduleOnStation;
        private IShedulePointPoint _shedulePointPoint;
        private IListStationsRoute _listStationsRoute;
        IMemoryCache _memoryCache;
        private CallMemoryDb _db;
        private GlobalSettings _conf;

        private double globalMinDist = 10000000.0; // минимальное расстояние до назначения, определяет что направление движения в ту сторону

        public TraceMethods(CoordinateDistance coords,
            IMemoryCache memoryCache,
            BrokenMessages messages,
            INearestStations nearestStations,
            ISheduleOnStation sheduleOnStation,
            IShedulePointPoint shedulePointPoint,
            IListStationsRoute listStationsRoute,
            IStationsList stationsList,
            IOptions<GlobalSettings> conf,
            CallMemoryDb db)
        {
            _coords = coords;
            _messages = messages;
            _nearestStations = nearestStations;
            _sheduleOnStation = sheduleOnStation;
            _shedulePointPoint = shedulePointPoint;
            _listStationsRoute = listStationsRoute;
            _memoryCache = memoryCache;
            _db = db;
            _conf = conf.Value;
        }
        /// <summary>
        /// проверка наличия станций
        /// </summary>
        public bool isHaveNearestStation(NearestStationsStruct stationsStruct)
        {
            if (stationsStruct.stations != null)
                return (stationsStruct.pagination.total >= 1);
            return false;
        }
        public bool isHaveSheduleStation(SheduleOnStationStrust stationsStruct)
        {
            if (stationsStruct.schedule != null)
                return (stationsStruct.schedule.Count >= 1);
            return false;
        }
        public bool isHavePointPoint(ShedulePointPointStruct stationsStruct)
        {
            if (stationsStruct.segments != null)
                return (stationsStruct.segments.Count >= 1);
            return false;
        }
        public bool isHaveRouteStationsList(ListStationsRouteStruct stationsStruct)
        {
            if (stationsStruct.stops != null)
                return (stationsStruct.stops.Count >= 1);
            return false;
        }

        /// <summary>
        /// определяет оптимальную транзитную станцию из списка станций по ветке
        /// </summary>
        public Stops GetOptimalTransitStationInUid(ListStationsRouteStruct stations, DateTime d, string currentStationCode)
        {
            var countStations = stations.stops.Count;
            var stop = new Stops();
            var minDist = globalMinDist;
            for (int i = 0; i < countStations -1 ; i++)
            {
                var coorByStation = _db.FindStationByCodeInMemory(stations.stops[i].station.code);
                if (coorByStation.settlementTitle == null)
                {
                    continue;
                }
                var coorTo = new double[] { _to.latitude, _to.langitude };
                var currentStation = _db.FindStationByCodeInMemory(currentStationCode);
                var coorCurrStation = new double[] { currentStation.stationLatitude, currentStation.stationLongitude };
                var coor = new double[] { coorByStation.stationLatitude, coorByStation.stationLongitude };
                var distFromFindStationToTo = _coords.DistanceBetweenTwoCoords(coor, coorTo);
                var distFromCurrentStationToTo = _coords.DistanceBetweenTwoCoords(coorCurrStation, coorTo);
                var departureTime = new DateConvert(stations.stops[i].departure).ConvertFromLongStringDateToDate();

                if (distFromCurrentStationToTo > distFromFindStationToTo &&
                    distFromFindStationToTo < minDist &&
                    d < departureTime)
                {
                    minDist = distFromFindStationToTo;
                    stop = stations.stops[i];
                }

            }
            globalMinDist = minDist;
            return stop;
        }

        /// <summary>
        /// сортирует ветки по близости конечной станции к пункту назначения
        /// </summary>
        private SheduleOnStationStrust SortSheduleOnStation(SheduleOnStationStrust stations, DateTime d)
        {
            var countStations = stations.schedule.Count;
            var sortStations = new SheduleOnStationStrust();
            int[] distMatrixKey = new int[countStations];
            double[] distMatrixValue = new double[countStations];
            var j = 0;
            for (int i = 0; i < countStations; i++)
            {
                var endCity = new TitlesConvert(stations.schedule[i].thread.title).GetSecondPointInDeparture();
                var coorByStation = _db.FindStationByCityAndTransportInMemory(endCity, stations.schedule[i].thread.transport_type);
                if (coorByStation.settlementTitle == null)
                {
                    continue;
                }
                var coorTo = new double[] { _to.latitude, _to.langitude };
                var coor = new double[] { coorByStation.stationLatitude, coorByStation.stationLongitude };
                var distFromFindStationToTo = _coords.DistanceBetweenTwoCoords(coor, coorTo);
                if (stations.schedule[i].departure != null)
                {
                    if (new DateConvert(stations.schedule[i].departure).ConvertFromLongStringDateToDate() > d)
                    {
                        distMatrixKey[j] = i;
                        distMatrixValue[j++] = distFromFindStationToTo;
                    }
                }
            }
            Array.Resize(ref distMatrixKey, j);
            Array.Resize(ref distMatrixValue, j);
            Array.Sort(distMatrixValue, distMatrixKey);
            sortStations = stations;
            var shedule = new List<Schedule>();
            for (int i = 0; i < j; i++)
            {
                shedule.Add(stations.schedule[distMatrixKey[i]]);
            }
            sortStations.schedule = shedule;
            return sortStations;
        }

        /// <summary>
        /// определяет оптимальную транзитную станцию по всем веткам
        /// </summary>
        public async Task<Stops> GetOptimalNextStationAsync(SheduleOnStationStrust stations, DateTime d)
        {
            var bestNextStation = new Stops();
            var minDist = 1000000.0;
            for (int uid = 0; uid < stations.schedule.Count; uid++)
            {
                var stationsRoutelist = new ListStationsRouteStruct();
                var temp = await FindListStationsRouteStringFromYandexAsync(stations.schedule[uid].thread.uid, d);
                if (!temp.Contains("Mistake::"))
                {
                    stationsRoutelist = JsonConvert.DeserializeObject<ListStationsRouteStruct>(temp);
                }
                else { continue; }
                var optimalStop = GetOptimalTransitStationInUid(stationsRoutelist, d, stations.station.code);
                if (optimalStop.station != null && minDist >= globalMinDist)
                {
                    bestNextStation = optimalStop;
                    minDist = globalMinDist;
                    _lastRouteStationsList = stationsRoutelist;
                    if (globalMinDist == 0) break;
                }
                else 
                {
                    continue; 
                }
               
            }
            
            return bestNextStation;
        }

        /// <summary>
        /// определяет оптимальное по времени отправление
        /// </summary>
        public Segments GetOptimalPointPoint(ShedulePointPointStruct stations, DateTime minDepartureTime)
        {
            var countStations = stations.segments.Count;
            var minPointPoint = new Segments();
            var minDuration = 10000000.0;
            for (int i = 0; i < countStations; i++)
            {
                var departure = new DateConvert(stations.segments[i].departure);

                if (departure.ConvertFromLongStringDateToDate() > minDepartureTime.AddMinutes(20) && stations.segments[i].duration < minDuration)
                {
                        minPointPoint = stations.segments[i];
                        minDuration = stations.segments[i].duration;
                }
            }

            return minPointPoint;
        }

        /// <summary>
        /// сортирует ближайшие станции по признаку важности
        /// </summary>
        public List<Stations> GetMajorityNearestStationList(NearestStationsStruct stations)
        {
            var countStations = stations.stations.Count;
            var majorityStations = new List<Stations>();
            var k = 0;
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 0; j < countStations; j++)
                {
                    if (Int32.TryParse(stations.stations[j].majority, out k) && (k == i))
                    {
                        majorityStations.Add(stations.stations[j]);
                    }
                }
            }
            

            return majorityStations;
        }

        /// <summary>
        /// получает список станций следования к привязке ко станции отправления
        /// </summary>
        public List<Stops> GetPointPointRouteList(ListStationsRouteStruct stations, string beginStation, string endStation)
        {
            var countStops = stations.stops.Count;
            var stops = new List<Stops>();
            int i = 0;
            int j = countStops;
            for (int k = 0; k < countStops; k++)
            {
                if (stations.stops[k].station.code == beginStation) i = k;
                if (stations.stops[k].station.code == endStation) j = k;
            }
            if (j >= i)
            {
                for (int k = i; k <= j; k++)
                {
                    stops.Add(stations.stops[k]);
                }
            }
            else
            {
                for (int k = i; k >= j; k--)
                {
                    stops.Add(stations.stops[k]);
                }
            }

            return stops;
        }

        /// <summary>
        /// ищет ближайшие станции
        /// </summary>
        public async Task<string> FindNearestStationsStringFromYandexAsync(double km, double lat, double lng, string transportType, string stationType)
        {
            try
            {
                var nearestStationsList = await _nearestStations.GetNearestStationsFromYandexServiceAsync(km, lat, lng, transportType, stationType);
                if (isHaveNearestStation(nearestStationsList))
                {
                    return JsonConvert.SerializeObject(nearestStationsList);
                }
                else
                {
                    return "Mistake::No stations in radius down " + km + " km";
                }

            }
            catch (BrokenCircuitException)
            {
                return _messages.NearestStationsInoperativeMsg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "NULL";
        }

        /// <summary>
        /// ищет ветки отправления от ближайшей станции
        /// </summary>
        public async Task<string> FindSheduleOnStationStringFromYandexAsync(string station, DateTime date, string transportType)
        {
            try
            {
                var sheduleOnStationList = await _sheduleOnStation.GetSheduleOnStationFromYandexServiceAsync(station, date, transportType);
                if (isHaveSheduleStation(sheduleOnStationList))
                {
                    return JsonConvert.SerializeObject(sheduleOnStationList);
                }
                else
                {
                    return "Mistake::No transit stations";
                }

            }
            catch (BrokenCircuitException)
            {
                return _messages.SheduleOnStationInoperativeMsg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "NULL";
        }

        /// <summary>
        /// ищет отправления по шаблону станция-станция
        /// </summary>
        public async Task<string> FindShedulePointPointStringFromYandexAsync(string from, string to, DateTime date, string transportType)
        {
            try
            {
                var pointPointList = await _shedulePointPoint.GetShedulePointPointFromYandexServiceAsync(from, to, date, transportType);
                if (isHavePointPoint(pointPointList))
                {
                    return JsonConvert.SerializeObject(pointPointList);
                }
                else
                {
                    return "Mistake::No station in thread";
                }

            }
            catch (BrokenCircuitException)
            {
                return _messages.PointPointInoperativeMsg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "NULL";
        }

        /// <summary>
        /// ищет станции следования по ветке OnShedule
        /// </summary>
        public async Task<string> FindListStationsRouteStringFromYandexAsync(string uid, DateTime date)
        {
            try
            {
                var stationsRoute = await _listStationsRoute.GetListStationsRouteFromYandexServiceAsync(uid, date);
                if (isHaveRouteStationsList(stationsRoute))
                {
                    return JsonConvert.SerializeObject(stationsRoute);
                }
                else
                {
                    return "Mistake::No stops in thread";
                }

            }
            catch (BrokenCircuitException)
            {
                return _messages.ListStationsRouteInoperativeMsg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "NULL";
        }

        /// <summary>
        /// главная функция поиска маршрута
        /// </summary>
        public async Task<string> FindStationByYandexAsync(double km, LatLngStruct coorFrom, LatLngStruct coorTo, string transportType, string date)
        {
            try
            {
                _transportTypes = new TitlesConvert(transportType).GetTransportTypeList();
                _stationTypes = new TitlesConvert("").CreateStationTypes(_transportTypes);
                _from = coorFrom;
                _to = coorTo;
                _route = new List<RouteList>();
                DateTime needDate;
                if (!DateTime.TryParse(date, out needDate))
                {
                    return "incorrect date";
                };
                if (needDate <= DateTime.Now.Date)
                {
                    needDate = DateTime.Now;
                }
                var lastNeedDate = needDate;
                _lastRouteStationsList = new ListStationsRouteStruct();
                var nextCity = "";
                var transportNumber = 0;
               var optimalThread = new Segments();



                while (nextCity != _to.city) 
                {
                    
                    var nearestStations = new NearestStationsStruct();
                    if (transportNumber >= _transportTypes.Count) 
                    {
                        //если на тек. день нет отправлений - брать след. день - до 2 дней
                        needDate = needDate.Date.AddDays(1);
                        if (lastNeedDate.AddDays(3) < needDate)
                        {
                            break;
                        }
                        transportNumber = 0;
                    }
                    if (nextCity == "" && transportNumber < _transportTypes.Count)
                    {

                        var temp = await FindNearestStationsStringFromYandexAsync(km, _from.latitude, _from.langitude, _transportTypes[transportNumber], _stationTypes[transportNumber]);
                            if (!temp.Contains("Mistake::"))
                            {
                                nearestStations = JsonConvert.DeserializeObject<NearestStationsStruct>(temp);
                            }
                            else { transportNumber++; continue; }
                    }
                    if (nextCity != "" && transportNumber < _transportTypes.Count)
                    {
                        var coorByStation = _db.FindStationByCodeInMemory(optimalThread.to.code);
                        var temp = await FindNearestStationsStringFromYandexAsync(km, coorByStation.stationLatitude, coorByStation.stationLongitude, 
                            _transportTypes[transportNumber], _stationTypes[transportNumber]);
                        if (!temp.Contains("Mistake::"))
                        {
                            nearestStations = JsonConvert.DeserializeObject<NearestStationsStruct>(temp);
                        }
                        else { transportNumber++; continue; }
                    }

                    //получение списка ближайших станций, отсортированных по важности
                    var majorityStations = GetMajorityNearestStationList(nearestStations);

                    for (var k = 0; k < majorityStations.Count; k++)
                    {
                        if (k == majorityStations.Count -1)
                            transportNumber++;
                        
                        var ns = majorityStations[k];
                        var timeInWay = ns.distance / _conf.MiddleVelocity;
                        var d = needDate.AddHours(timeInWay);
                        var sheduleOnStationList = new SheduleOnStationStrust();

                        //поиск всех веток по станции
                        var temp = await FindSheduleOnStationStringFromYandexAsync(ns.code, d, ns.transport_type);
                        if (!temp.Contains("Mistake::"))
                        {
                            sheduleOnStationList = JsonConvert.DeserializeObject<SheduleOnStationStrust>(temp);
                        }
                        else { continue; }
                        //сортировка веток
                        var sortSheduleOnStationList = SortSheduleOnStation(sheduleOnStationList, d);
                        var sort = sortSheduleOnStationList;
                        //поиск оптимальной станции в пути следования и передача ее как следующей nextStation
                        var nextStation = await GetOptimalNextStationAsync(sortSheduleOnStationList, d);
                        if (nextStation.station == null)
                        {
                            continue;
                        }
                        var nextCityTitle = _db.FindStationByCodeInMemory(nextStation.station.code).settlementTitle;
                        var pointPointList = new ShedulePointPointStruct();
                        
                        //поиск отправления
                        temp = await FindShedulePointPointStringFromYandexAsync(ns.code, nextStation.station.code, d, ns.transport_type);
                        if (!temp.Contains("Mistake::"))
                        {
                            pointPointList = JsonConvert.DeserializeObject<ShedulePointPointStruct>(temp);
                        }
                        else { continue; }
                        var optimal = GetOptimalPointPoint(pointPointList, d);
                        if (optimal.departure == null)
                        {
                            continue;
                        }
                        else
                        {
                            var way = new RouteList();
                            optimalThread = optimal;
                            way.route = optimalThread;
                            way.stops = GetPointPointRouteList(_lastRouteStationsList, ns.code, nextStation.station.code);
                            // добавление отправления в путь
                            _route.Add(way);
                            nextCity = nextCityTitle;
                            needDate = new DateConvert(optimalThread.departure).ConvertFromLongStringDateToDate().AddSeconds(optimalThread.duration);
                            lastNeedDate = needDate;
                            transportNumber = 0;
                            break;
                        }
                        
                    }
                }
                if (nextCity == _to.city)
                {
                    return JsonConvert.SerializeObject(_route);
                }
                else 
                {
                    return "Маршрут построен не полностью:\n" + JsonConvert.SerializeObject(_route);

                }
                    
                     
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            


        }
    }
}
