using M87.SimulatorCore.Interfaces;
using M87.SimulatorCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Engine
{
    public class OrderManager
    {
        private IOrderBook _orderBook;
        private ILogger _logger;

        public OrderManager(IOrderBook orderBook, ILogger logger)
        {
            _orderBook = orderBook;
            _logger = logger;
            _orderBook.OnOrderMatched += HandleOrderMatched;
        }

        public void SubmitOrder(Order order)
        {
            // Validazione dell'ordine
            if (!ValidateOrder(order))
            {
                _logger.Log($"Ordine non valido: {order.OrderId}");
                return;
            }

            // Inserimento nell'order book
            _orderBook.AddOrder(order);
            _logger.Log($"Ordine inserito: {order.OrderId}, {order.Side} {order.Quantity} @ {order.Price}");
        }

        private bool ValidateOrder(Order order)
        {
            // Implementare le regole di validazione
            if (order.Quantity <= 0)
                return false;
            if ((order.Type == OrderType.Limit || order.Type == OrderType.StopLimit) && order.Price <= 0)
                return false;
            // Altre validazioni...
            return true;
        }

        private void HandleOrderMatched(Order bid, Order ask)
        {
            _logger.Log($"Ordine eseguito: Buy {bid.OrderId} e Sell {ask.OrderId} per {ask.Price} x {Math.Min(bid.Quantity, ask.Quantity)}");
            // Aggiornare il prezzo corrente dello stock se necessario
            // Ad esempio, notificare il MarketSessionManager o l'OrderBook
        }
    }
}
