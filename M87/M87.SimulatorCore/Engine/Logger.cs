using M87.SimulatorCore.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Engine
{
    public class Logger : Interfaces.ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public Logger(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public Logger()
        {
                
        }

        public void Log(string message)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] {message}");
            // In alternativa, integra un framework di logging come NLog o Serilog// Puoi personalizzare il formato del messaggio come preferisci
            if (_logger != null)
                _logger.LogInformation($"[{DateTime.UtcNow}] {message}");
        }
    }
}
