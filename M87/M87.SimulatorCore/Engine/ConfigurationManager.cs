using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Engine
{
    public class ConfigurationManager
    {
        public List<string> StockSymbols { get; set; } = new List<string>();
        public double Drift { get; set; } = 0.0001;
        public double Volatility { get; set; } = 0.01;
        public TimeSpan CandleInterval { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan TickInterval { get; set; } = TimeSpan.FromMilliseconds(1000);
        public TimeSpan DeltaTime { get; set; } = TimeSpan.FromMinutes(1);
        // Aggiungi altre impostazioni configurabili secondo necessità
    }
}
