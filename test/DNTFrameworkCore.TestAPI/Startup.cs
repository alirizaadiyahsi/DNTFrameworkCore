﻿using DNTFrameworkCore.EntityFramework;
using DNTFrameworkCore.FluentValidation;
using DNTFrameworkCore.TestAPI.Application;
using DNTFrameworkCore.TestAPI.Hubs;
using DNTFrameworkCore.TestAPI.Infrastructure;
using DNTFrameworkCore.TestAPI.Infrastructure.Context;
using DNTFrameworkCore.TestAPI.Resources;
using DNTFrameworkCore.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace DNTFrameworkCore.TestAPI
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
            services.AddDNTFramework()
                .AddModelValidation()
                .AddFluentModelValidation()
                .AddTransaction(options =>
                {
                    // options.Timeout=TimeSpan.FromMinutes(3);
                    //options.IsolationLevel=IsolationLevel.ReadCommitted;
                });

            services.AddDNTProtectionRepository<ProjectDbContext>();
            services.AddDNTCommonWeb().AddDNTDataProtection();

            services.AddInfrastructure(Configuration);
            services.AddApplication(Configuration);
            services.AddResources();
            services.AddWebAPI();
            services.AddJwtAuthentication(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Use(async (context, next) =>
                    {
                        var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;
                        if (error?.Error is SecurityTokenExpiredException)
                        {
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                            {
                                Message = "authentication token expired"
                            }));
                        }
                        else if (error?.Error != null)
                        {
                            context.Response.StatusCode = 500;
                            context.Response.ContentType = "application/json";
                            const string message = "متأسفانه مشکلی در فرآیند انجام درخواست شما پیش آمده است!";

                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                            {
                                Message = message
                            }));
                        }
                        else
                        {
                            await next();
                        }
                    });
                });
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseDNTFramework();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
            app.UseSignalR(routes => { routes.MapHub<NotificationHub>("/api/notificationhub"); });
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}