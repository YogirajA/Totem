using AutoMapper;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using MediatR;
using Totem.Infrastructure;
using Totem.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using OdeToCode.AddFeatureFolders;

namespace Totem
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var featureFolderOptions = new FeatureFolderOptions();

            services.AddMvc(option=> option.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddFeatureFolders(featureFolderOptions)
                .WithRazorPagesRoot($"/{featureFolderOptions.FeatureFolderName}")
                .AddFluentValidation(cfg => { cfg.RegisterValidatorsFromAssemblyContaining<Startup>(); });
            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup));
            services.AddDbContext<TotemContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("Database")));

            services.AddScoped<TesterService>();

            services.Configure<EmailSettings>(Configuration.GetSection(nameof(EmailSettings)));
            services.AddScoped<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IMapper mapper)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                ConfigureDevOnlyExceptionVisibility(app, env);
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        private void ConfigureDevOnlyExceptionVisibility(IApplicationBuilder app, IHostingEnvironment env)
        {
            var isDebug = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);

            if (isDebug)
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            // TODO: standard error message (CT-53)
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //}
        }
    }
}
