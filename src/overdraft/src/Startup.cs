using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Google.Cloud.Diagnostics.AspNetCore;
using Google.Cloud.Diagnostics.Common;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    public class Startup
    {
        private readonly string _projectId;
        private readonly string _version; 

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _projectId = Configuration["GOOGLE_PROJECT_ID"];
            _version = Configuration["VERSION"];
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add google exception logging
            services.AddGoogleExceptionLogging(options =>
            {
                options.ProjectId = _projectId;
                options.ServiceName = "overdraftservice";
                options.Version = _version;
            });

            // Add trace service.
            services.AddGoogleTrace(options =>
            {
                options.ProjectId = _projectId;
                options.Options = TraceOptions.Create(
                    bufferOptions: BufferOptions.NoBuffer());
            });

            // BankOfAnthos specific services used with dependency injection.
            services.AddSingleton<IBankService, BankService>();
            services.AddScoped<IOverdraftRepository, FirestoreOverdraftRepository>();
            services.AddScoped<IOverdraftService, OverdraftService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(_version, new OpenApiInfo { 
                    Title = "Anthos.Samples.BankOfAnthos.Overdraft", 
                    Version = _version });
            });

            ConfigureJwtAuth(services);

            // Setup DI for http client (recommended), also adds GoogleTracing.
            services.AddHttpClient<IBankService, BankService>()
                .AddOutgoingGoogleTraceHandler();            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => 
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "overdraft v1");
                    c.RoutePrefix = string.Empty;
                });
            }
            else
            {
                app.UseHttpsRedirection();
            }

            loggerFactory.AddGoogle(app.ApplicationServices, _projectId);

            // Configure error reporting service.
            app.UseGoogleExceptionLogging();
            // Configure trace service.
            app.UseGoogleTrace();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureJwtAuth(IServiceCollection services)
        {
            JwtHelper jwtHelper = new JwtHelper(Configuration);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.IncludeErrorDetails = true;
                x.SaveToken = true;
                x.TokenValidationParameters = jwtHelper.GetJwtValidationParameters();
            });
        }
    }
}
