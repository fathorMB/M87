using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Models;
using M87.SimulatorCore.Simulation;
using M87.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace M87.WebAPI.Services
{
    public class SimulationHostedService : IHostedService
    {
        private readonly ILogger<SimulationHostedService> _logger;
        private readonly MarketSessionManager _marketSessionManager;
        private readonly SimulatorCore.Engine.TimeProvider _timeProvider;

        public SimulationHostedService(ILogger<SimulationHostedService> logger, ISimulationEventHandler simulationEventHandler)
        {
            _logger = logger;

            // Configurazione della simulazione
            DateTime startTime = DateTime.UtcNow;
            TimeSpan tickInterval = TimeSpan.FromSeconds(1); // Ogni secondo
            TimeSpan deltaTime = TimeSpan.FromSeconds(1); // Incremento di tempo per ogni tick

            // Configurazione delle azioni da simulare
            List<Stock> stocks = new List<Stock>
            {
                new Stock("AAPL", 150.0, new GeometricBrownianMotionSimulator(0.0001, 0.01), new Logger(_logger))
                // Puoi aggiungere altre azioni qui
            };

            _timeProvider = new SimulatorCore.Engine.TimeProvider(startTime, tickInterval, deltaTime);
            _marketSessionManager = new MarketSessionManager(_timeProvider, stocks, TimeSpan.FromMinutes(1), new Logger(_logger), new List<ISimulationEventHandler> { simulationEventHandler });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Avvio della simulazione di mercato.");

            // Avvia la sessione di mercato
            _marketSessionManager.StartSession();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Arresto della simulazione di mercato.");

            // Ferma la sessione di mercato
            _marketSessionManager.StopSession();

            return Task.CompletedTask;
        }
    }
}
