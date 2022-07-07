using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace StatefulWebService
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables("STWS_");

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            var app = builder.Build();

            var logger = app.Logger;

            var dataDir = app.Configuration["DataDir"];
            if (string.IsNullOrEmpty(dataDir))
            {
                dataDir = "/etc/stws-data/";
            }

            Directory.CreateDirectory(dataDir);

            logger.LogInformation($"Data directory is set to {dataDir}");

            const string FileName = "data.txt";
            var filePath = Path.Combine(dataDir, FileName);

            logger.LogInformation($"Data file path is {filePath}");

            app.MapGet("/", () =>
            {
                string data;
                if (File.Exists(filePath))
                {
                    data = File.ReadAllText(filePath);
                }
                else
                {
                    data = "No data is saved.";
                }

                return string.Join(Environment.NewLine, $"This is {Environment.MachineName} machine.", $"Saved instance data: {data}");
            });

            app.MapPost("/", async c =>
            {
                using var reader = new StreamReader(c.Request.Body);
                var data = await reader.ReadToEndAsync();

                await File.WriteAllTextAsync(filePath, data);

                var response = $"Data was saved on {Environment.MachineName} machine.";

                c.Response.StatusCode = StatusCodes.Status200OK;
                await c.Response.WriteAsync(response);
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
}