using Dapper;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System;
using TraceRouteApi.Settings;
using TraceRouteApi.Models.StationsList;
using TraceRouteApi.Models.GlobalModels;
using Microsoft.Extensions.Caching.Memory;
using TraceRouteApi.Services;
using TraceRouteApi.Mappers;
using System.Transactions;
namespace TraceRouteApi.Repositories
{
    public class CallMemoryDb
    {
        private ConnectionSettings _conf;
        IMemoryCache _memoryCache;
        private IStationsList _stationsList;

        public CallMemoryDb(IOptions<ConnectionSettings> conf,
            IMemoryCache memoryCache,
            IStationsList stationsList)
        {
            _conf = conf.Value;
            _memoryCache = memoryCache;
            _stationsList = stationsList;
        }

        /// <summary>
        /// загружает список станций из БД в MemoryCache
        /// </summary>
        public async Task LoadStationsListInMemoryAndDbAsync()
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(60));
            var stationsList = await _stationsList.GetStationsListFromYandexServiceAsync();
            var stationsMapper = new StationsListToRussianStationsList(stationsList);
            var russianStationsList = stationsMapper.ToRussianStationsList();
            _memoryCache.Set("StationsList", russianStationsList, cacheEntryOptions);

            await SetStationsListInDbAsync(russianStationsList);
        }

        /// <summary>
        /// загружает список станций в БД
        /// </summary>
        private async Task SetStationsListInDbAsync(List<RussianStationsListStruct> list)
        {
            
                using (var conn = new SqlConnection(_conf.connString))
                {
                    await conn.OpenAsync();
                    var trans = conn.BeginTransaction();
                try
                {
                    var sql = @"INSERT INTO StationsList (regionTitle, settlementTitle, stationCode, stationName, transportType, stationLongitude, stationLatitude) 
                        VALUES (@regionTitle, @settlementTitle, @stationCode, @stationName, @transportType, @stationLongitude, @stationLatitude)";
                    var str = await conn.ExecuteAsync(sql, list, transaction: trans);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                }
            }
            
        }

        /// <summary>
        /// получает список станций из БД
        /// </summary>
        private async Task<List<RussianStationsListStruct>> GetStationsListFromDbAsync()
        {
            var stations = new List<RussianStationsListStruct>();
            var sql = "SELECT * FROM StationsList";
            try
            {
                using (var conn = new SqlConnection(_conf.connString))
                {
                    using (var multi = await conn.QueryMultipleAsync(sql))
                    {
                        var order = multi.Read<RussianStationsListStruct>().AsList();
                        stations.AddRange(order);

                        return stations;
                    }
                }
                
            }
            catch (Exception ex)
            {

            }
            return stations;
        }

        /// <summary>
        /// загружает список станций из БД в MemoryCache
        /// </summary>
        public async Task LoadStationsListInMemoryFromDbAsync()
        {
            try
            {
                var stations = await GetStationsListFromDbAsync();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(60));
                _memoryCache.Remove("StationsList");
                _memoryCache.Set("StationsList", stations, cacheEntryOptions);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// ищет первое упоминание о станции в памяти 
        /// по названию города и типу транспорта
        /// </summary>
        public RussianStationsListStruct FindStationByCityAndTransportInMemory(string settlement, string transportType)
        {
            var station = new RussianStationsListStruct();
            var stations = new List<RussianStationsListStruct>();
            try
            {
                if (_memoryCache.TryGetValue("StationsList", out stations))
                {

                        return FindStationByCity(stations, settlement, transportType);
                    
                }
            }
            catch (Exception ex)
            {

            }
            return station;
        }

        /// <summary>
        /// ищет станцию в памяти по коду станции
        /// </summary>
        public RussianStationsListStruct FindStationByCodeInMemory(string stationCode)
        {
            var station = new RussianStationsListStruct();
            var stations = new List<RussianStationsListStruct>();
            try
            {
                if (_memoryCache.TryGetValue("StationsList", out stations))
                {
                    return FindStationByCode(stations, stationCode);
                }
            }
            catch (Exception ex)
            {

            }
            return station;
        }

        /// <summary>
        /// ищет станцию в структуре по названию города и типу транспорта
        /// </summary>
        private RussianStationsListStruct FindStationByCity(List<RussianStationsListStruct> stations, 
            string settlement,
            string transportType)
        {
            var station = new RussianStationsListStruct();
            int i = 0;
            while (i < stations.Count)
            {
                if (transportType == "all")
                {
                    if (stations[i].settlementTitle == settlement &&
                        stations[i].stationLatitude != 0 &&
                        stations[i].stationLongitude != 0)
                    {
                        return stations[i];
                    }
                }
                else
                {
                    if (stations[i].settlementTitle == settlement &&
                            stations[i].transportType == transportType &&
                            stations[i].stationLatitude != 0 &&
                            stations[i].stationLongitude != 0)
                    {
                        return stations[i];
                    }
                }
                i++;
            }
            return station;
        }

        /// <summary>
        /// ищет станцию в структуре по коду станции
        /// </summary>
        private RussianStationsListStruct FindStationByCode(List<RussianStationsListStruct> stations,
            string stationCode)
        {
            var station = new RussianStationsListStruct();
            int i = 0;
            while (i < stations.Count)
            {
                if (stations[i].stationCode == stationCode)
                {
                    return stations[i];
                }
                i++;
            }
            return station;
        }

        /// <summary>
        /// ищет координаты города по названию в MemoryCache
        /// </summary>
        public LatLngStruct FindCoordByCityTitle(string city)
        {
            var stationInCity = FindStationByCityAndTransportInMemory(city,"all");
            if (stationInCity != null)
            {
                var res = new LatLngStruct();
                res.city = stationInCity.settlementTitle;
                res.langitude = stationInCity.stationLongitude;
                res.latitude = stationInCity.stationLatitude;
                return res;
            }
            return new LatLngStruct();
        }
    }
}
