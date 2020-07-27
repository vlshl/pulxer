using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using Common;
using System.Net.WebSockets;
using Pulxer.Leech;
using System.Text;
using Microsoft.AspNetCore.Authentication;

namespace WebApp
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var jwtConfig = _config.GetSection("JwtToken");
            string key = DataProtect.TryUnProtect(jwtConfig.GetValue("Key", AuthOptions.KEY));

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
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
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(key),
                        ValidateIssuerSigningKey = true
                    };
                });

            services.AddControllers();
            
            string pulxerConnectionString = DataProtect.TryUnProtect(_config.GetConnectionString("Pulxer"));
            string leechConnectionString = DataProtect.TryUnProtect(_config.GetConnectionString("Leech"));
            Pulxer.BL.ConfigureServices(services, _config, pulxerConnectionString, leechConnectionString);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // defaults
            var fp = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "pxapp"));
            var dfo = new DefaultFilesOptions();
            dfo.DefaultFileNames.Clear();
            dfo.DefaultFileNames.Add("index.html");
            dfo.FileProvider = fp;
            app.UseDefaultFiles(dfo);

            // static
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fp,
                RequestPath = "",
                OnPrepareResponse = ctx =>
                {
                    if (ctx.Context.Request.Path.Value == "/index.html")
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                    }
                }
            });

            if (env.IsDevelopment())
            {
                app.UseCors(b =>
                {
                    b.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost:4200")
                    .AllowCredentials();
                });
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                string path = context.Request.Path.ToString();
                if (path.StartsWith("/ws"))
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var authRes = await context.AuthenticateAsync();
                        if (!authRes.Succeeded || !context.User.IsInRole("leech"))
                        {
                            context.Response.StatusCode = 401;
                            return;
                        }

                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var lsm = context.RequestServices.GetRequiredService<LeechServerManager>();
                        var ls = lsm.CreateServer(new LeechPipeServerSocket(webSocket));
                        if (ls != null)
                        {
                            await ls.Run();
                            ls.Close();
                            lsm.DeleteServer();
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
