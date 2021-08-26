using System;
using System.Threading.Tasks;
using Food.Data;
using Food.Services.Config;
using Food.Services.Utils;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Food.Services.ShedulerQuartz;
using Food.Services.Filters;
using Food.Services.Services;
using Serilog;
using Common.Serilog.Formatters;
using Common.Serilog.Sinks.LogstashHttp;
using DinkToPdf;
using DinkToPdf.Contracts;
using Common.Serilog.Middleware;
using Common.Serilog.ActionFilters;
using Microsoft.AspNetCore.HttpOverrides;

namespace Food.Services
{
    public class Startup
    {
        private readonly IConfiguration _rawConfig;
        private readonly IConfigureSettings _appConfig;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _rawConfig = configuration;
            _appConfig = _rawConfig.Get<ConfigureSettings>();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Accessor.ConnectionString = _appConfig.ConnectionStrings.DefaultConnection;
            Accessor.LoggerFactory = loggerFactory;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Внедрение зависимости (DI) для отчетов PDF
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddLogging(builder => {
                builder.ClearProviders();
                builder.AddSerilog();
            });

            services.AddAuthentication(auth => {
                auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
                auth.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.RequireHttpsMetadata = _appConfig.AuthToken.RequireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _appConfig.AuthToken.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _appConfig.AuthToken.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = _appConfig.AuthToken.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                };
                //TODO: Авторизация через соцсети. Удалить
                //}).AddGoogle(options => {
                //    options.Scope.Add("email");
                //    options.CallbackPath = new PathString("/api/Account/ExternalLogin?provider=Google");
                //    options.ClientId = _rawConfig.GetSection("Providers:Google:ClientId").Value;
                //    options.ClientSecret = _rawConfig.GetSection("Providers:Google:ClientSecret").Value;
                //}).AddVkontakte(options => {
                //    options.Scope.Add("email");
                //    options.ClientId = _rawConfig.GetSection("Providers:Vk:ClientId").Value;
                //    options.ClientSecret = _rawConfig.GetSection("Providers:Vk:ClientSecret").Value;
                //}).AddFacebook(options => {
                //    options.Scope.Add("email");

                //    options.CallbackPath = new PathString("/api/Account/ExternalLogin?provider=Facebook");
                //    options.AppId = _rawConfig.GetSection("Providers:Facebook:ClientId").Value;
                //    options.AppSecret = _rawConfig.GetSection("Providers:Facebook:ClientSecret").Value;
            }).AddCookie(OpenIdConnectDefaults.AuthenticationScheme);

            services.AddHttpContextAccessor();
            services.AddMvc(options => {
                options.Filters.Add<MvcLoggingEnricher>();
                options.Filters.Add<ReshapeErrorResponseFilter>();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHttpContextAccessor();
            services.AddSingleton(_ => _appConfig);
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IFoodScheduler>(_ => ShedulerQuartz.Scheduler.Instance);
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IFoodContext, FoodContext>();
            services.AddTransient<Accessor>();
            services.AddTransient<IMenuService, MenuService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCustomRequestLogging();
            app.UseAuthentication();
            app.UseDeveloperExceptionPage();
            app.UseMvcWithDefaultRoute();

            ConfigureScheduler(app.ApplicationServices).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task ConfigureScheduler(IServiceProvider sp)
        {
            var scheduler = sp.GetRequiredService<IFoodScheduler>();

            await scheduler.Start(sp, _appConfig.ConnectionStrings.DefaultConnection).ConfigureAwait(false);
            await scheduler.ScheduleApplicationDiagnosticsTask().ConfigureAwait(false);
            await scheduler.ScheduleCreateCompanyOrdersTask().ConfigureAwait(false);
            await scheduler.ScheduleDishInMenuEntityCleanupTask().ConfigureAwait(false);

            // Запустить задачи планировщика при старте проекта (для отладки).
            //await scheduler.RunCreateCompanyOrdersTaskNow().ConfigureAwait(false);
            //await scheduler.RunDispatchIndividualOrderNotificationTaskNow(11707);
        }
    }
}
