
using CoinTradeAppMVC.Web.Models;

namespace CoinTradeAppMVC.Services
{
	public class OrderTradeService : IOrderTradeService
	{
		private readonly AppDbContext _dbContext;

        public OrderTradeService(AppDbContext dbContext)
        {
			_dbContext = dbContext;
        }

        public async Task BuyCoinAsync(int userId, string coinSymbol, decimal quantity, decimal price)
		{
			await _dbContext.SaveChangesAsync();
		}

		public async Task SellCoinAsync(int userId, string coinSymbol, decimal quantity, decimal price)
		{
			await _dbContext.SaveChangesAsync();
		}
	}
}
