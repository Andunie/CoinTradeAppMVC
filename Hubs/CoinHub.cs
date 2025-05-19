using Binance.Spot;
using CoinTradeAppMVC.ApiModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Elfie.Model;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.Extensions.Logging;
using CoinTradeAppMVC.ViewModels;

namespace CoinTradeAppMVC.Hubs
{
    public class CoinHub : Hub
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Market _market;

        public CoinHub(IHttpClientFactory httpClientFactory, Market market)
        {
            _httpClientFactory = httpClientFactory;
            _market = market;
        }

		// Portfolio
        public async Task SendCurrentPrice(string symbol)
        {
            var jsonSymbolPriceTickerData = await _market.SymbolPriceTicker(symbol);
            var symbolPriceTicker = JsonConvert.DeserializeObject<SymbolPriceTickerModel>(jsonSymbolPriceTickerData);
            decimal coinPrice = decimal.Parse(symbolPriceTicker!.Price!, System.Globalization.CultureInfo.InvariantCulture);


            await Clients.All.SendAsync("ReceiveCoinUpdate", coinPrice, symbol);
        }

		// ViewAllCoins
		public async Task SendCoinData(string symbol)
		{
			string jsonResponse = await _market.TwentyFourHrTickerPriceChangeStatistics();
			var tickerPriceChangeStatisticData = JsonConvert.DeserializeObject<List<TickerResponse>>(jsonResponse);

			var filteredData = tickerPriceChangeStatisticData
				?.Where(x => !string.IsNullOrEmpty(x.LastPrice) &&
				decimal.Parse(x.LastPrice, CultureInfo.InvariantCulture) > 0 &&
				x.Symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase))
				.ToList();

			var selectedSymbolData = filteredData
				.FirstOrDefault(x => x.Symbol.Equals(StringComparison.OrdinalIgnoreCase));

			foreach (var coin in filteredData)
			{
				string coinSymbol = coin.Symbol;
				decimal coinPrice = decimal.Parse(coin.LastPrice, CultureInfo.InvariantCulture);
				decimal coinVolume = decimal.Parse(coin.Volume, CultureInfo.InvariantCulture);
				decimal coinChange = decimal.Parse(coin.PriceChangePercent, CultureInfo.InvariantCulture);

				await Clients.All.SendAsync("ReceiveCoinData", coinPrice, coinVolume, coinChange, coinSymbol);
			}
		}

		// /Member/Index
		public async Task SendHomeCoinData()
		{
			var coins = new List<HomeCoinViewModel>
			{
				new HomeCoinViewModel { Symbol = "BTC", Name = "Bitcoin", LogoUrl = "https://cryptologos.cc/logos/bitcoin-btc-logo.png" },
				new HomeCoinViewModel { Symbol = "ETH", Name = "Ethereum", LogoUrl = "https://cryptologos.cc/logos/ethereum-eth-logo.png" },
				new HomeCoinViewModel { Symbol = "BNB", Name = "BNB", LogoUrl = "https://cryptologos.cc/logos/binance-coin-bnb-logo.png" },
				new HomeCoinViewModel { Symbol = "XRP", Name = "XRP", LogoUrl = "https://cryptologos.cc/logos/xrp-xrp-logo.png" }
			};

			foreach (var coin in coins)
			{
				string jsonResponse = await _market.CurrentAveragePrice($"{coin.Symbol.ToUpper()}USDT");
				var currentPriceData = JsonConvert.DeserializeObject<CurrentAveragePriceModel>(jsonResponse);
				coin.Price = currentPriceData!.Price;

				string symbol = $"{coin.Symbol.ToUpper()}USDT";
				string tickerString = await GetTicker(symbol);
				coin.ChangePercent = Decimal.Parse(tickerString, CultureInfo.InvariantCulture) / 1000;
			}

			await Clients.All.SendAsync("ReceiveIndexHomeCoinData", coins);
		}

		public async Task<string> GetTicker(string symbol)
		{
			var client = new HttpClient();
			var url = $"https://api.binance.com/api/v3/ticker/24hr?symbol={symbol}";

			var response = await client.GetAsync(url);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				var tickerResponse = JsonConvert.DeserializeObject<TickerResponse>(content);
				return tickerResponse!.PriceChangePercent;
			}
			else
			{
				throw new Exception($"Binance API error: {response.StatusCode}");
			}
		}
	}
}