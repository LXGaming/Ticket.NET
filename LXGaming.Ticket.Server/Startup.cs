using System.Text.Json;
using System.Text.Json.Serialization;
using LXGaming.Ticket.Server.Security;
using LXGaming.Ticket.Server.Security.Authentication;
using LXGaming.Ticket.Server.Services.Event;
using LXGaming.Ticket.Server.Storage;
using LXGaming.Ticket.Server.Storage.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace LXGaming.Ticket.Server {

    public class Startup {

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services) {
            services.AddAuthentication(TokenBearerDefaults.AuthenticationScheme).AddTokenBearer();
            services.AddAuthorization(options => {
                options.AddPolicy(SecurityConstants.Policies.Scope, builder => {
                    builder.RequireClaim(SecurityConstants.Claims.Scope);
                });
            });

            services.AddDbContext<StorageContext, MySqlStorageContext>();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers().AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
            services.AddHealthChecks();
            services.AddSingleton<EventService>();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Ticket API", Version = "v1"}); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            using var scope = app.ApplicationServices.CreateScope();
            using var storageContext = scope.ServiceProvider.GetService<StorageContext>();
            storageContext?.Database.EnsureCreated();

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticket API v1"));
            }

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health").RequireHost("127.0.0.1");
            });
        }
    }
}