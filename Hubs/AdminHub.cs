using Binance.Spot;
using CoinTradeAppMVC.ApiModels;
using CoinTradeAppMVC.Web.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace CoinTradeAppMVC.Hubs
{
    public class AdminHub : Hub
    {
        private readonly AppDbContext _db;
        private readonly Market _market;

        public AdminHub(AppDbContext db, Market market)
        {
            _db = db;
            _market = market;
        }

        public async Task RequestUserDashboard(string userId)
        {
            var portfolioRaw = await _db.UserPortfolio
                .Where(p => p.UserId == userId)
                .GroupBy(p => p.Symbol)
                .Select(g => new {
                    Symbol = g.Key,
                    TotalAmount = g.Sum(p => p.Amount),
                    AvgPrice = g.Average(p => p.Price)
                }).ToListAsync();

            var cash = await _db.UserCash.Where(x => x.UserId == userId).Select(x => x.Balance).FirstOrDefaultAsync();

            decimal totalCurrentValue = 0m;
            decimal totalCost = 0m;

            var portfolio = new List<object>();
            foreach (var item in portfolioRaw.Where(x => x.TotalAmount > 0))
            {
                string symbol = item.Symbol;
                decimal avgPrice = item.AvgPrice;
                decimal amount = item.TotalAmount;

                var json = await _market.SymbolPriceTicker(symbol);
                var ticker = JsonConvert.DeserializeObject<SymbolPriceTickerModel>(json);
                decimal currentPrice = decimal.Parse(ticker.Price!, CultureInfo.InvariantCulture);
                decimal currentValue = currentPrice * amount;
                decimal cost = avgPrice * amount;

                portfolio.Add(new { symbol, value = Math.Round(currentValue, 2) });

                totalCurrentValue += currentValue;
                totalCost += cost;
            }

            var profitLoss = totalCurrentValue - totalCost;

            await Clients.Caller.SendAsync("ReceiveUserDashboard", new
            {
                portfolio,
                cash,
                profitLoss
            });
        }
    }
}