namespace CoinTradeAppMVC.Services
{
	public interface IOrderTradeService
	{
		Task BuyCoinAsync(int userId, string coinSymbol, decimal quantity, decimal price);
	}
}
