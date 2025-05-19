namespace CoinTradeAppMVC.ViewModels
{
	public class HomeCoinViewModel
	{
		public string Symbol { get; set; }
		public string Name { get; set; }
		public string LogoUrl { get; set; }
		public decimal Price { get; set; }
		public decimal ChangePercent { get; set; }
	}
}
