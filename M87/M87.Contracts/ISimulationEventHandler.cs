using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.Contracts
{
    public interface ISimulationEventHandler
    {
        Task OnPriceUpdateAsync(PriceUpdate priceUpdate);
    }
}
