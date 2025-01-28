using M87.Contracts;
using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Models;
using M87.SimulatorCore.Simulation;

public class SimulationHostedService : IHostedService
{
    private readonly ILogger<SimulationHostedService> _logger;
    private readonly MarketSessionManager _marketSessionManager;

    public SimulationHostedService(ILogger<SimulationHostedService> logger, ISimulationEventHandler simulationEventHandler)
    {
        _logger = logger;
        // Configura la simulazione e il gestore degli eventi
        // Assicurati che MarketSessionManager riceva gli handler corretti
        _marketSessionManager = new MarketSessionManager(
            new M87.SimulatorCore.Engine.TimeProvider(DateTime.UtcNow, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)),
            new List<Stock>
            {
                new Stock("AAPL", 150.0, new GeometricBrownianMotionSimulator(0.0001, 0.01), new Logger(_logger))
            },
            TimeSpan.FromMinutes(1),
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
