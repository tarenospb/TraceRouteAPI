using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TraceRouteApi.Settings;
using TraceRouteApi.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using TraceRouteApi.Validators;
using TraceRouteApi.Repositories;
using TraceRouteApi.Models.GlobalModels;

namespace TraceRouteApi.Controllers
{
    [Route("api/route")]
    public class GetRouteController : Controller
    {
        private readonly ILogger<GetRouteController> _logger;
        private BrokenMessages _messages;
        private TraceMethods _methods;
        private CallMemoryDb _db;
        IMemoryCache _memoryCache;

        public GetRouteController(ILogger<GetRouteController> logger,
            IMemoryCache memoryCache,
            BrokenMessages messages,
            TraceMethods methods,
            CallMemoryDb db)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _methods = methods;
            _messages = messages;
            _db = db;
        }


            [HttpPost]
        public async Task<IActionResult> GetRoute([FromBody] UserSettings userSettings)
        {
            try
            {
                UserSettingsValidator usValidator = new UserSettingsValidator(_messages);
                var set = userSettings;
                var validResult = usValidator.Validate(set);
                if (validResult.IsValid)
                {
                    var fromCity = set.fromCity;
                    var km = set.radius;
                    var toCity = set.toCity;
                    var type = set.transportType;
                    var date = set.date;

                    await _db.LoadStationsListInMemoryFromDbAsync();
                    var coorFrom = _db.FindCoordByCityTitle(fromCity);
                    var coorTo = _db.FindCoordByCityTitle(toCity);
                    if (coorFrom.city == null || coorTo.city == null)
                    {
                        return Ok(_messages.WrongCityMsg);
                    }
                    else 
                    {
                        var mes = await _methods.FindStationByYandexAsync(km, coorFrom, coorTo, type, date);
                        return Ok(mes);
                    }

                    

                }
                else 
                {
                    return BadRequest(validResult.Errors);
                }
                    
            }
            
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
            return Ok("no directions");
        }

    }
}
