using CoinTradeAppMVC.ApiModels;

namespace CoinTradeAppMVC.ViewModels
{
    public class AllCoinsViewModel
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Change {  get; set; }
        public decimal Volume { get; set; }
		public string LogoUrl { get; set; }
	}

    public class ViewAllCoinsViewModel
    {
        public List<AllCoinsViewModel> TickerData { get; set; } 
    }
}
