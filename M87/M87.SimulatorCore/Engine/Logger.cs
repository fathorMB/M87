using M87.SimulatorCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Engine
{
    public class Logger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] {message}");
            // In alternativa, integra un framework di logging come NLog o Serilog
        }
    }
}
