namespace CoinTradeAppMVC.ApiModels
{
    public class CurrentAveragePriceModel
    {
        public int Mins { get; set; }
        public decimal Price { get; set; }
        public long CloseTime { get; set; }
    }
}
