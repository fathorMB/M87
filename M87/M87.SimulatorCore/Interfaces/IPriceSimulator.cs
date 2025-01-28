using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Interfaces
{
    public interface IPriceSimulator
    {
        double SimulateNextPrice(double currentPrice, double deltaTime);
    }
}
