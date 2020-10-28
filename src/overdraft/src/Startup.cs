using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;

namespace Anthos.Samples.BankOfAnthos.Overdraft
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
            services.AddSingleton<IBankService, BankService>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "Anthos.Samples.BankOfAnthos.Overdraft", 
                    Version = "v1" });
            });

            ConfigureJwtAuth(services);
        }

        private void ConfigureJwtAuth(IServiceCollection services)
        {
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
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = GetJwtKey(),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });                        
        }

        private SecurityKey GetJwtKey()
        {
            const string JWT_SECRET_NAME = "JwtSecret";

            string secret = Configuration[JWT_SECRET_NAME];
            if (string.IsNullOrEmpty(secret))
                throw new ApplicationException($"Missing JWT Key: {JWT_SECRET_NAME}");

            byte[] bytes = Convert.FromBase64String(secret);
            string publicKey = Encoding.UTF8.GetString(bytes);

            publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", "");
            publicKey = publicKey.Replace("-----END PUBLIC KEY-----", "");
            publicKey = publicKey.Replace("\r", "");
            publicKey = publicKey.Replace(Environment.NewLine, "");

            RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(
                source: Convert.FromBase64String(publicKey),
                bytesRead: out int _
            );
            
            return new RsaSecurityKey(rsa);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
