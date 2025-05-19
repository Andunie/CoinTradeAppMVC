using Newtonsoft.Json;

namespace CoinTradeAppMVC.ApiModels
{
    public class TickerResponse
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("priceChange")]
        public string PriceChange { get; set; }

        [JsonProperty("priceChangePercent")]
        public string PriceChangePercent { get; set; }

        [JsonProperty("weightedAvgPrice")]
        public string WeightedAvgPrice { get; set; }

        [JsonProperty("prevClosePrice")]
        public string PrevClosePrice { get; set; }

        [JsonProperty("lastPrice")]
        public string LastPrice { get; set; }

        [JsonProperty("lastQty")]
        public string LastQty { get; set; }

        [JsonProperty("bidPrice")]
        public string BidPrice { get; set; }

        [JsonProperty("bidQty")]
        public string BidQty { get; set; }

        [JsonProperty("askPrice")]
        public string AskPrice { get; set; }

        [JsonProperty("askQty")]
        public string AskQty { get; set; }

        [JsonProperty("openPrice")]
        public string OpenPrice { get; set; }

        [JsonProperty("highPrice")]
        public string HighPrice { get; set; }

        [JsonProperty("lowPrice")]
        public string LowPrice { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }

        [JsonProperty("quoteVolume")]
        public string QuoteVolume { get; set; }

        [JsonProperty("openTime")]
        public long OpenTime { get; set; }

        [JsonProperty("closeTime")]
        public long CloseTime { get; set; }

        [JsonProperty("firstId")]
        public long FirstId { get; set; }

        [JsonProperty("lastId")]
        public long LastId { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }
    }
}
