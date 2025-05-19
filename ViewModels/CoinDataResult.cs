using CoinTradeAppMVC.ApiModels;

namespace CoinTradeAppMVC.ViewModels
{
	public class CoinDataResult
	{
		public List<KlineViewModel>? KlineList { get; set; }
		public string? Patterns { get; set; }
	}
}
