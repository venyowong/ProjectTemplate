using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectTemplate.Factories;
using ProjectTemplate.Helpers;
using ProjectTemplate.Middlewares;
using ProjectTemplate.Services;
using StackExchange.Redis;

namespace ProjectTemplate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            #if (Swagger)
            services.AddSwaggerDocument();
            #endif
            #if (RateLimit)
            this.AddRateLimit(services);
            #endif
            services.Configure<Configuration>(this.Configuration);

            services.AddHttpClient<GitHubService>();
            
            services.AddSingleton<DbConnectionFactory>()
                .AddSingleton(_ => ConnectionMultiplexer.Connect(this.Configuration["Redis:ConnectionString"]))
                .AddTransient(serviceProvider =>
                {
                    int.TryParse(this.Configuration["Redis:DefaultDb"], out int db);
                    return serviceProvider.GetService<ConnectionMultiplexer>().GetDatabase(db);
                })
                .AddTransient(serviceProvider => new RedisService(serviceProvider.GetService<IDatabase>()));

            services.AddCors(o => o.AddPolicy("Default", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            #if (Quartz)
            services.AddSingleton<IJobFactory, CustomJobFactory>();
            services.AddHostedService<QuartzHostedService>();

            services.AddTransientBothTypes<IScheduledJob, HelloJob>();
            #endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<Configuration> configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                #if (Swagger)
                app.UseOpenApi();
                app.UseSwaggerUi3();
                #endif
            }
            app.UseStaticFiles();

            app.UseMiddleware<LogMiddleware>();
            #if (Quartz)
            app.UseMiddleware<ManualTriggerMiddleware>();
            #endif

            #if (RateLimit)
            app.UseIpRateLimiting();
            #endif

            app.UseCors("Default");
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            DapperHelper.MakeDapperMapping("ProjectTemplate.Models");
        }

        #if (RateLimit)
        private void AddRateLimit(IServiceCollection services)
        {
            // needed to load configuration from appsettings.json
            services.AddOptions();

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            // https://github.com/aspnet/Hosting/issues/793
            // the IHttpContextAccessor service is not registered by default.
            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }
        #endif
    }
}
