using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using TraceRouteApi.Settings;
using TraceRouteApi.Services;
using TraceRouteApi.Mappers;
using TraceRouteApi.Validators;
using TraceRouteApi.Repositories;
using FluentValidation.AspNetCore;

namespace TraceRouteApi
{
    public class Startup
    {
        public Startup(IConfiguration conf)
        {
            Configuration = conf;
        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(3, TimeSpan.FromMinutes(5));
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                            retryAttempt)));
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddControllers()
                .AddFluentValidation(fv =>
                {
                    fv.ImplicitlyValidateChildProperties = true;
                    fv.ImplicitlyValidateRootCollectionElements = true;
                    fv.RegisterValidatorsFromAssemblyContaining<Startup>();
                });
            services.Configure<GlobalSettings>(option => Configuration.GetSection("GlobalSettings").Bind(option));
            services.Configure<ConnectionSettings>(option => Configuration.GetSection("ConnectionSettings").Bind(option));
            services.AddTransient<BrokenMessages>();
            services.AddTransient<UserSettings>();
            services.AddTransient<UserSettingsValidator>();
            services.AddTransient<CoordinateDistance>();
            services.AddTransient<CallMemoryDb>();
            services.AddTransient<TraceMethods>();
            services.AddTransient<StationsListToRussianStationsList>();
            services.AddTransient<DateConvert>();
            services.AddTransient<TitlesConvert>();
            services.AddHttpClient<INearestStations, NearestStations>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(1))
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
            services.AddHttpClient<IStationsList, StationsList>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(3))
                .AddPolicyHandler(GetRetryPolicy());
            services.AddHttpClient<ISheduleOnStation, SheduleOnStation>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(3))
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
            services.AddHttpClient<IShedulePointPoint, ShedulePointPoint>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(3))
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
            services.AddHttpClient<IListStationsRoute, ListStationsRoute>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(3))
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
