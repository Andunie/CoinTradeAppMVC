namespace CoinTradeAppMVC.Areas.Admin.Models
{
	public class BalancesPageViewModel
	{
		public IEnumerable<UserBalanceViewModel>? Balances { get; set; }
		public UserBalanceViewModel NewUserBalance { get; set; } = new UserBalanceViewModel();
	}
}
