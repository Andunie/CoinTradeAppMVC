namespace CoinTradeAppMVC.ViewModels
{
    public class MarginsPageViewModel
    {
        public IEnumerable<UserMarginViewModel> Margins { get; set; }
        public UserMarginViewModel NewMargin { get; set; } = new UserMarginViewModel();
    }
}
