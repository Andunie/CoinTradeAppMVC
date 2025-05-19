using CoinTradeAppMVC.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoinTradeAppMVC.ViewModels
{
	public class OrdersPageViewModel
	{
		public IEnumerable<UserOrderViewModel>? Orders { get; set; }
		public UserOrderViewModel NewOrder { get; set; } = new UserOrderViewModel();
	}	
}
