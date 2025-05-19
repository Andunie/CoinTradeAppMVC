namespace CoinTradeAppMVC.Areas.Admin.Models
{
	public class MarginBalancesPageViewModel
	{
		public IEnumerable<UserMarginBalanceViewModel>? MarginBalances { get; set; }
		public UserMarginBalanceViewModel NewUserMarginBalance { get; set; } = new UserMarginBalanceViewModel();
	}
}
