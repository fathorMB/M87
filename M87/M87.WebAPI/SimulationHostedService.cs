// File: M87/M87.WebAPI/SimulationHostedService.cs

using M87.Contracts;
using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Models;
using M87.SimulatorCore.Simulation;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class SimulationHostedService : IHostedService
{
    private readonly ILogger<SimulationHostedService> _logger;
    private readonly MarketSessionManager _marketSessionManager;

    public SimulationHostedService(ILogger<SimulationHostedService> logger, ISimulationEventHandler simulationEventHandler)
    {
        _logger = logger;
        // Configura la simulazione e il gestore degli eventi
        _marketSessionManager = new MarketSessionManager(
            new M87.SimulatorCore.Engine.TimeProvider(DateTime.UtcNow),
            new List<Stock>
            {
                new Stock("AAPL", 150.0, new GeometricBrownianMotionSimulator(0.0001, 0.01), new Logger(_logger))
            },
            new Logger(_logger),
            new List<ISimulationEventHandler> { simulationEventHandler }
        );
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Avvio della simulazione di mercato.");
        _marketSessionManager.StartSession();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Arresto della simulazione di mercato.");
        _marketSessionManager.StopSession();
        return Task.CompletedTask;
    }
}
