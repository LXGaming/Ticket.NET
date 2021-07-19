using System;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File.Archive;

namespace LXGaming.Ticket.Server {

    public static class Program {

        public static void Main(string[] args) {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine("logs", "server-.log"),
                    buffered: true,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 1,
                    hooks: new ArchiveHooks(CompressionLevel.Optimal))
                .CreateLogger();

            try {
                Log.Information("Initializing...");
                CreateHostBuilder(args).Build().Run();
            } catch (Exception ex) {
                Log.Fatal(ex, "Application failed to initialize");
            } finally {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
        }
    }
}