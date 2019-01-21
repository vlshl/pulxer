using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Pulxer;

namespace WebApp
{
    public class Startup
    {
        private IConfiguration _config;
        private IHostingEnvironment _environment;

        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var jwtConfig = _config.GetSection("JwtToken");
            string key = DataProtect.TryUnProtect(jwtConfig.GetValue("Key", AuthOptions.KEY));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    //opt.RequireHttpsMetadata = _environment.IsProduction();
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtConfig.GetValue("Issuer", AuthOptions.ISSUER),
                        ValidateAudience = true,
                        ValidAudience = jwtConfig.GetValue("Audience", AuthOptions.AUDIENCE),
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(key),
                        ValidateIssuerSigningKey = true
                    };
                });

            string pulxerConnectionString = DataProtect.TryUnProtect(_config.GetConnectionString("Pulxer"));
            string leechConnectionString = DataProtect.TryUnProtect(_config.GetConnectionString("Leech"));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            Pulxer.BL.ConfigureServices(services, pulxerConnectionString, leechConnectionString);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
            app.UseStaticFiles();
        }
    }
}
