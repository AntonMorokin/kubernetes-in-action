using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;

namespace SampleWebService
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Configuration.AddEnvironmentVariables("sws_");

            var app = builder.Build();

            var pathBase = app.Configuration["networking:pathbase"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
                app.UseRouting();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.MapGet("/weather-forecast", () =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateTime.Now.AddDays(index),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))
                    .ToArray();

                return forecast;
            })
            .WithName("GetWeatherForecast");

            app.MapGet("/system-info", () =>
            {
                return $"This is {Environment.MachineName} machine.";
            });

            var healthProbeNumber = 0;

            app.MapGet("/health", () =>
            {
                return ++healthProbeNumber < 5
                    ? Results.Ok()
                    : Results.StatusCode(StatusCodes.Status500InternalServerError);
            });

            var started = DateTime.Now;

            app.MapGet("/ready", () =>
            {
                return (DateTime.Now - started).TotalSeconds < 15
                    ? Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
                    : Results.Ok();
            });

            app.MapFallback((HttpContext context) =>
            {
                return $"Can't find appropriate handler. Path base: {context.Request.PathBase}; path: {context.Request.Path}";
            });

            app.Run();
        }
    }

    internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}