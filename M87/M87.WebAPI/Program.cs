
using M87.Contracts;
using M87.WebAPI.Hubs;

namespace M87.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            // Configura CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .WithOrigins("http://localhost:4200") // Indirizzo del tuo frontend Angular
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            // Registrare le interfacce e implementazioni
            builder.Services.AddSingleton<ISimulationEventHandler, SimulationEventHandler>();

            // Registrare il servizio host per la simulazione
            builder.Services.AddHostedService<SimulationHostedService>();

            builder.Services.AddEndpointsApiExplorer();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<PriceHub>("/hubs/price");

            app.Run();
        }
    }
}
