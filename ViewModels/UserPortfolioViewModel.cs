namespace CoinTradeAppMVC.ViewModels
{
	public class UserPortfolioViewModel
	{
		public string Symbol { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal TotalCost { get; set; }
		public decimal TotalValue { get; set; }
		public decimal CurrentPrice { get; set; }
		public decimal ProfitLoss { get; set; } // Kâr/Zarar
	}
}
