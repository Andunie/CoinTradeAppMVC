using Binance.Spot;
using CoinTradeAppMVC.ApiModels;
using CoinTradeAppMVC.Entities;
using CoinTradeAppMVC.ViewModels;
using CoinTradeAppMVC.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using static NuGet.Packaging.PackagingConstants;
using System.Drawing.Printing;

namespace CoinTradeAppMVC.Controllers
{
    [Authorize]
	[Route("Member")]
	public class MemberController : Controller
	{
		private readonly SignInManager<AppUser> _signInManager;
		private readonly UserManager<AppUser> _userManager;
		private readonly HttpClient _httpClient;
		private readonly AppDbContext _dbContext;
		private readonly Market _market;
		//private static KlineViewModel _currentKline;
		private MarketDataWebSocket _webSocket;
		//private readonly IHubContext<CoinHub> _hubContext;
		private readonly StackExchange.Redis.IDatabase _cache;
        private readonly IStringLocalizer<MemberController> _localizer;

        public MemberController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, HttpClient httpClient, AppDbContext dbContext, Market market,
			/*IHubContext<CoinHub> hubContext*/ IConnectionMultiplexer redis, IStringLocalizer<MemberController> localizer)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_httpClient = httpClient;
			_dbContext = dbContext;
			_market = market;
			//_hubContext = hubContext;
			_cache = redis.GetDatabase();
            _localizer = localizer;
        }

		[HttpGet("Logout")]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();

			return RedirectToAction("Home", "Home");
		}

		[HttpGet("Index")]
		public async Task<IActionResult> Index()
		{
			var coins = new List<HomeCoinViewModel>
			{
				new HomeCoinViewModel {Symbol = "BTC", Name= "Bitcoin", LogoUrl = "https://cdn.jsdelivr.net/gh/simplr-sh/coin-logos/images/bitcoin/large.png"},
				new HomeCoinViewModel {Symbol = "ETH", Name= "Ethereum", LogoUrl = "https://cdn.jsdelivr.net/gh/simplr-sh/coin-logos/images/ethereum/large.png"},
				new HomeCoinViewModel {Symbol = "BNB", Name= "BNB", LogoUrl = "https://cdn.jsdelivr.net/gh/simplr-sh/coin-logos/images/binancecoin/large.png"},
				new HomeCoinViewModel {Symbol = "XRP", Name= "XRP", LogoUrl = "https://cdn.jsdelivr.net/gh/simplr-sh/coin-logos/images/ripple/large.png"}
			};

			foreach (var coin in coins)
			{
				string jsonResponse = await _market.CurrentAveragePrice($"{coin.Symbol.ToUpper()}USDT");	
				var currentPriceData = JsonConvert.DeserializeObject<CurrentAveragePriceModel>(jsonResponse);
				coin.Price = currentPriceData!.Price;

				string symbol = $"{coin.Symbol.ToUpper()}USDT";
				coin.ChangePercent = System.Convert.ToDecimal(await GetTicker(symbol));
				coin.ChangePercent = coin.ChangePercent / 1000;
			}

			var userId = _userManager.GetUserId(User);

			var portfolio = await _dbContext.UserPortfolio
				.Where(x => x.UserId == userId)
				.GroupBy(x => x.Symbol)
				.Select(g => new
				{
					Symbol = g.Key,
					TotalAmount = g.Sum(p => p.Amount)
				})
				.Where(x => x.TotalAmount > 0)
				.ToListAsync();

			// Grafik verisi için hesaplama
			var chartData = portfolio
				.Select(x => new
				{
					symbol = x.Symbol,
					amount = x.TotalAmount
				}).ToList();

			ViewBag.PortfolioChartData = JsonConvert.SerializeObject(chartData);

			return View(coins);
		}

		public async Task<string> GetTicker(string symbol)
		{
			var client = new HttpClient();
			var url = $"https://api.binance.com/api/v3/ticker?symbol={symbol}";

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

		public IActionResult CoinData()
		{
			return View();
		}

		[HttpGet("CoinData/{symbol}")]
		public async Task<IActionResult> CoinData(string symbol)
		{
			ViewBag.Symbol = symbol;

			if (string.IsNullOrEmpty(symbol))
			{
				return View("Error", new ErrorViewModel { ErrorMessage = "Symbol is required." });
			}

			// Redis'te veriyi saklamak için cache key belirle
			string cacheKey = $"coinData:{symbol}";
			string cachedResult = null;
			List<KlineViewModel> klineList = null;
			string patternsResult = null;

			// Redis'ten önbelleğe alınmış veriyi kontrol et
			try
			{
				cachedResult = await _cache.StringGetAsync(cacheKey);
				if (!string.IsNullOrEmpty(cachedResult))
				{
					// Eğer önbellekte varsa, deserialize edip view'e gönder
					var cachedData = JsonConvert.DeserializeObject<CoinDataResult>(cachedResult);
					ViewBag.Patterns = cachedData.Patterns;
					return View(cachedData.KlineList);
				}
			}
			catch (Exception ex)
			{
				// Redis erişimi başarısız olursa, önbelleği atla ve logla
				Console.WriteLine($"Redis cache access failed: {ex.Message}. Proceeding without cache.");
			}

			// 2 ay önceki tarihi ve şu anki tarihi al
			DateTime startDate = DateTime.UtcNow.AddMonths(-1); // 2 ay önce
			DateTime endDate = DateTime.UtcNow; // Bugün

			// Binance API için interval değeri (1 saatlik interval)
			string interval = "1h"; // 1 saatlik mumlar

			// Unix zaman damgası hesapla
			long startTime = DateTimeOffset.FromUnixTimeMilliseconds(
				(long)startDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
			).ToUnixTimeMilliseconds();
			long endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			// API URL oluştur
			string url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval={interval}&startTime={startTime}&endTime={endTime}&limit=1000";

			using (var client = new HttpClient())
			{
				try
				{
					var response = await client.GetStringAsync(url);
					if (string.IsNullOrEmpty(response))
					{
						return View("Error", new ErrorViewModel { ErrorMessage = "No candlestick data available." });
					}

					var candlestickData = JsonConvert.DeserializeObject<List<List<object>>>(response);
					if (candlestickData == null || !candlestickData.Any())
					{
						return View("Error", new ErrorViewModel { ErrorMessage = "No candlestick data available." });
					}

					klineList = candlestickData.Select(k => new KlineViewModel
					{
						OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(System.Convert.ToInt64(k[0])).UtcDateTime,
						OpenPrice = decimal.Parse(k[1].ToString()!, CultureInfo.InvariantCulture),
						HighPrice = decimal.Parse(k[2].ToString()!, CultureInfo.InvariantCulture),
						LowPrice = decimal.Parse(k[3].ToString()!, CultureInfo.InvariantCulture),
						ClosePrice = decimal.Parse(k[4].ToString()!, CultureInfo.InvariantCulture),
						Volume = decimal.Parse(k[5].ToString()!, CultureInfo.InvariantCulture),
						CloseTime = DateTimeOffset.FromUnixTimeMilliseconds(System.Convert.ToInt64(k[6])).UtcDateTime,
						NumberOfTrades = System.Convert.ToInt32(k[8])
					}).ToList();
				}
				catch (Exception ex)
				{
					return View("Error", new ErrorViewModel { ErrorMessage = $"Error fetching candlestick data: {ex.Message}" });
				}

				// FastAPI'ye POST isteği yap
				try
				{
					var jsonContent = JsonConvert.SerializeObject(klineList);
					var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
					var fastApiUrl = "http://localhost:8001/api/patterns_detect";
					var apiResponse = await client.PostAsync(fastApiUrl, content);

					if (apiResponse.IsSuccessStatusCode)
					{
						patternsResult = await apiResponse.Content.ReadAsStringAsync();
						ViewBag.Patterns = patternsResult; // Pattern sonuçlarını ViewBag'e aktar
					}
					else
					{
						var errorMessage = await apiResponse.Content.ReadAsStringAsync();
						return View("Error", new ErrorViewModel { ErrorMessage = $"Pattern detection API error: {errorMessage}" });
					}
				}
				catch (Exception ex)
				{
					return View("Error", new ErrorViewModel { ErrorMessage = $"Error contacting pattern detection API: {ex.Message}" });
				}

				// Redis'e kaydet (isteğe bağlı, başarısız olursa atla)
				try
				{
					var coinDataResult = new CoinDataResult
					{
						KlineList = klineList,
						Patterns = patternsResult
					};
					await _cache.StringSetAsync(cacheKey, JsonConvert.SerializeObject(coinDataResult), TimeSpan.FromMinutes(10));
				}
				catch (Exception ex)
				{
					// Redis'e yazma başarısız olursa, logla ve devam et
					Console.WriteLine($"Failed to cache data in Redis: {ex.Message}. Proceeding without caching.");
				}

				return View(klineList);
			}
		}

		public IActionResult BuyCoin()
		{
			return View();
		}

		[HttpPost("BuyCoin")]
		public async Task<IActionResult> BuyCoin(string symbol, string amount)
		{
			double convertedAmount = System.Convert.ToDouble(amount, System.Globalization.CultureInfo.InvariantCulture);

			var jsonSymbolPriceTickerData = await _market.SymbolPriceTicker(symbol);

			var symbolPriceTicker = JsonConvert.DeserializeObject<SymbolPriceTickerModel>(jsonSymbolPriceTickerData);

			if (symbolPriceTicker == null || string.IsNullOrWhiteSpace(symbolPriceTicker.Price))
			{
				TempData["WarningMessage"] = _localizer["FailedRetrievePriceMessage"].Value;
				return RedirectToAction("CoinData", symbol);
			}

			if (string.IsNullOrWhiteSpace(symbol) || convertedAmount <= 0)
			{
				TempData["WarningMessage"] = _localizer["InvalidDataProvidedMessage"].Value;
				return RedirectToAction("CoinData", symbol);
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				TempData["WarningMessage"] = _localizer["UserNotFoundMessage"].Value;
				return RedirectToAction("SignIn");
			}

			decimal coinPrice = decimal.Parse(symbolPriceTicker.Price, System.Globalization.CultureInfo.InvariantCulture);

			var userCash = await _dbContext.UserCash.FirstOrDefaultAsync(uc => uc.UserId == user.Id);
			if (userCash == null || userCash.Balance < (decimal)convertedAmount * coinPrice)
			{
                TempData["WarningMessage"] = _localizer["InsufficientBalanceMessage"].Value;
				return RedirectToAction("CoinData", "Member", new { symbol = symbol });
            }

			var transaction = new UserPortfolio
			{
				UserId = user.Id,
				Symbol = symbol,
				Amount = (decimal)convertedAmount,
				Price = coinPrice,
				TransactionDate = DateTime.UtcNow
			};

			_dbContext.UserPortfolio.Add(transaction);
			var transactionResult = await _dbContext.SaveChangesAsync();

			userCash.Balance -= (decimal)convertedAmount * coinPrice;
			userCash.LastUpdated = DateTime.UtcNow;

			_dbContext.UserCash.Update(userCash);
			var cashTransactionResult = await _dbContext.SaveChangesAsync();

            if (transactionResult > 0 && cashTransactionResult > 0)
			{
				TempData["SuccessMessage_"] = _localizer["TransactionSuccessfulMessage"].Value;
				return RedirectToAction("CoinData", "Member", new { symbol = symbol });
			}
			else
			{
				TempData["WarningMessage"] = _localizer["TransactionFailedMessage"].Value;
				return RedirectToAction("CoinData", "Member", new { symbol = symbol });
			}
		}

		// User Menu
		[HttpGet("UserMenu")]
		public async Task<IActionResult> UserMenu()
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			var userViewModel = new UserViewModel
			{
				Email = currentUser!.Email,
				PhoneNumber = currentUser.PhoneNumber,
				UserName = currentUser.UserName
			};

			return View(userViewModel);
		}

		[HttpPost("PasswordChange")]
		public async Task<IActionResult> PasswordChange(UserViewModel request)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser!= null)
			{
				var checkOldPassword = await _userManager.CheckPasswordAsync(currentUser, request.Password!);

				if (!checkOldPassword)
				{
					ModelState.AddModelError(string.Empty, _localizer["PasswordDoesNotMatchWithOldPasswordMessage"].Value);
					return View();
				}

				string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
				var result = await _userManager.ResetPasswordAsync(currentUser, passwordResetToken, request.NewPassword!);

				if (result.Succeeded)
				{
					TempData["SuccessMessage"] = _localizer["PasswordHasBeenChangedSuccessfulyMessage"].Value;
					return RedirectToAction("UserMenu", "Member");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			return RedirectToAction("UserMenu", "Member");
		}

		[HttpGet("UserPortfolio")]
		public async Task<IActionResult> UserPortfolio()
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser == null)
			{
				return RedirectToAction("SignIn");
			}

			var userCash = await _dbContext.UserCash
				.FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

			// Bu kod eklenmeden önce oluşturulan kullanıcılar için burası hata verecektir.
			if (userCash == null)
			{
				ModelState.AddModelError(string.Empty, _localizer["UserCashDataNotFoundMessage"].Value);
				return View();
			}

			var userPortfolio = await _dbContext.UserPortfolio
									.Where(p => p.UserId == currentUser.Id)
									.GroupBy(p => p.Symbol)
									.ToListAsync();  // Asenkron listeyi alıyoruz

			var userPortfolioViewModel = new List<UserPortfolioViewModel>();

			foreach (var group in userPortfolio)
			{
				var currentPrice = await GetCurrentPrice(group.Key);

				// Kullanıcı portföyü için ViewModel oluşturuyoruz
				var portfolio = new UserPortfolioViewModel
				{
					Symbol = group.Key,
					TotalAmount = group.Sum(p => p.Amount),
					TotalCost = group.Sum(p => p.Amount * p.Price),
					CurrentPrice = currentPrice,
					TotalValue = group.Sum(p => p.Amount * currentPrice),
					ProfitLoss = group.Sum(p => p.Amount * currentPrice) - group.Sum(p => p.Amount * p.Price)
				};

				userPortfolioViewModel.Add(portfolio);
			}

			ViewBag.UserBalance = userCash.Balance;
				
			return View(userPortfolioViewModel);
		}

		[HttpGet("ViewAllCoins")]
		public async Task<IActionResult> ViewAllCoins()
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);
			if (currentUser == null)
				return RedirectToAction("SignIn");

			const string cacheKey = "allCoinsData";

			// 1) Önbellek kontrolü
			var cachedResult = await _cache.StringGetAsync(cacheKey);
			List<TickerResponse> tickerData;
			if (!string.IsNullOrEmpty(cachedResult))
			{
				tickerData = JsonConvert.DeserializeObject<List<TickerResponse>>(cachedResult)!;
			}
			else
			{
				var jsonResponse = await _market.TwentyFourHrTickerPriceChangeStatistics();
				tickerData = JsonConvert.DeserializeObject<List<TickerResponse>>(jsonResponse)!;

				// 2) Redis'e kaydet (1 dk)
				await _cache.StringSetAsync(cacheKey,
					JsonConvert.SerializeObject(tickerData),
					TimeSpan.FromMinutes(1));
			}

			// 3) Filtre ve LogoUrl eşlemesi
			var filtered = tickerData
				.Where(x => x.Symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase)
						 && x.LastPrice != "0.00000000")
				.Select(x =>
				{
					// "BTCUSDT" → "BTC"
					var baseSymbol = x.Symbol[..^4];
					// küçük harfe çevir
					var id = baseSymbol.ToLower() switch
					{
						"bnb" => "binancecoin",
						"xrp" => "ripple",
						// özel eşlemeler gerekiyorsa buraya ekleyin
						_ => baseSymbol.ToLower()
					};
					return new AllCoinsViewModel
					{
						Symbol  = x.Symbol,
						Price   = decimal.Parse(x.LastPrice, CultureInfo.InvariantCulture),
						Change  = decimal.Parse(x.PriceChangePercent, CultureInfo.InvariantCulture),
						Volume  = decimal.Parse(x.Volume, CultureInfo.InvariantCulture),
						LogoUrl = $"https://cdn.jsdelivr.net/gh/simplr-sh/coin-logos/images/{id}/small.png"
					};
				})
				.ToList();

			var vm = new ViewAllCoinsViewModel { TickerData = filtered };
			return View(vm);
		}

		[HttpGet("Orders")]
		public async Task<IActionResult> Orders()
		{
			// butonlar aracılığı ile girilen emri düzenleme ve silmek için yeni dialoglar oluşturulacaktır. 
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser == null)
			{
				return RedirectToAction("SignIn");
			}

			var orders = _dbContext.UserOrders
				.Where(o => o.UserId == _userManager.GetUserId(User))
				.Select(o => new UserOrderViewModel
				{
					Id = o.Id,
					CoinSymbol = o.Symbol,
					TargetPrice = o.TargetPrice,
					Quantity = o.Quantity,
					IsBuyOrder = o.IsBuyOrder,
					IsCompleted = o.IsCompleted
				})
				.Take(50) // Sadece ilk 50 kaydı al
				.ToList();

			var coins = _dbContext.CoinList
				.Select(c => new CoinList
				{
					Id = c.Id,
					CoinName = c.CoinName
				})
				.ToList();

			ViewBag.Coins = new SelectList(coins, "CoinName" ,"CoinName");

			var viewModel = new OrdersPageViewModel
			{
				Orders = orders,
			};

			return View(viewModel);
		}

		[HttpPost("Orders")]
		public async Task<IActionResult> Orders(OrdersPageViewModel request)
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser == null)
			{
				return RedirectToAction("SignIn");
			}

			var userCash = await _dbContext.UserCash
				.FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

			var coins = await _dbContext.CoinList
				.ToListAsync();
				

			var newOrder = new UserOrders
			{
				UserId = currentUser.Id,
				Symbol = request.NewOrder.CoinSymbol.ToUpper(),
				TargetPrice = (decimal)request.NewOrder.TargetPrice,
				Quantity = (decimal)request.NewOrder.Quantity,
				IsBuyOrder = request.NewOrder.IsBuyOrder,
				CreatedDate = DateTime.Now,
				IsCompleted = false
			};

			// Oluşturmada gerekli kontroller yapılır! 
			if (newOrder.IsBuyOrder == true)
			{
				if (userCash!.Balance < newOrder.TargetPrice * newOrder.Quantity)
				{
					TempData["NewOrderErrorMessage"] = _localizer["BalanceIsNotEnoughForOrderMessage"].Value;
					return RedirectToAction("Orders");
				}
			}
			else if (newOrder.IsBuyOrder == false)
			{
				// Kullanıcının portföyünde bu coin var mı ve miktarı yeterli mi kontrol ediliyor.
				var userPortfolio = await _dbContext.UserPortfolio
					.FirstOrDefaultAsync(p => p.UserId == currentUser.Id && p.Symbol == newOrder.Symbol);

				if (userPortfolio == null || userPortfolio.Amount < newOrder.Quantity)
				{
					TempData["NewOrderErrorMessage"] = _localizer["InsufficientCoinBalanceInPortfolioMessage"].Value;
					return RedirectToAction("Orders");
				}
			}

			_dbContext.UserOrders.Add(newOrder);
			await _dbContext.SaveChangesAsync();

			TempData["SuccessMessage"] = _localizer["OrderHasBeenCreatedSuccessfullyMessage"].Value;
			return RedirectToAction("Orders");
		}

		[HttpPost("DeleteOrder")]
		public async Task<IActionResult> DeleteOrder(int orderId)
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser == null)
			{
				return RedirectToAction("SignIn");
			}

			var order = await _dbContext.UserOrders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == currentUser.Id);

			if (order == null)
			{
				TempData["DeleteErrorMessage"] = _localizer["OrderNotFoundorNotAuthorizedtoDeleteMessage"].Value;
				return RedirectToAction("Orders");
			}

			await _dbContext.Database.ExecuteSqlRawAsync("EXEC sp_DeleteUserOrder {0}", orderId);

			TempData["SuccessMessage"] = _localizer["OrderHasBeenDeletedSuccessfullyMessage"].Value;
			return RedirectToAction("Orders");
		}

		[HttpPost("EditOrder")]
		public async Task<IActionResult> EditOrder(int orderId, OrdersPageViewModel request)
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser == null)
			{
				return RedirectToAction("SignIn");
			}

			var order = await _dbContext.UserOrders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == currentUser.Id);

			if (order == null)
			{
				TempData["EditErrorMessage"] = _localizer["OrderNotFoundMessage"].Value;
				return RedirectToAction("Orders");
			}

			order.UserId = currentUser.Id;
			order.Symbol = request.NewOrder.CoinSymbol.ToUpper();
			order.TargetPrice = (decimal)request.NewOrder.TargetPrice;
			order.Quantity = (decimal)request.NewOrder.Quantity;
			order.IsBuyOrder = request.NewOrder.IsBuyOrder;

			// Güncellemede gerekli kontroller yapılır! 
			if (order.IsBuyOrder == true)
			{
				var userCash = await _dbContext.UserCash
				.FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

				if (userCash!.Balance < order.TargetPrice * order.Quantity)
				{
					TempData["BalanceErrorMessage"] = _localizer["BalanceisNotEnoughForThisOrderMessage"].Value;
					return RedirectToAction("Orders");
				}
			}
			else if (order.IsBuyOrder == false)
			{
				// Kullanıcının portföyünde bu coin var mı ve miktarı yeterli mi kontrol ediliyor.
				var userPortfolio = await _dbContext.UserPortfolio
					.FirstOrDefaultAsync(p => p.UserId == currentUser.Id && p.Symbol == order.Symbol);

				if (userPortfolio == null || userPortfolio.Amount < order.Quantity)
				{
					TempData["NewOrderErrorMessage"] = _localizer["InsufficientCoinBalanceinPortfolioForOrderMessage"].Value;
					return RedirectToAction("Orders");
				}
			}

			await _dbContext.SaveChangesAsync();

			TempData["EditSuccessMessage"] = _localizer["OrderHasBeenEditedSuccessfullyMessage"].Value;
			return RedirectToAction("Orders");
		}

		[HttpPost("UserPortfolio")]
		public async Task<IActionResult> PortfolioBuyCoin(string symbol, string amount)
		{
			double convertedAmount = System.Convert.ToDouble(amount, System.Globalization.CultureInfo.InvariantCulture);

			var jsonSymbolPriceTickerData = await _market.SymbolPriceTicker(symbol);

			var symbolPriceTicker = JsonConvert.DeserializeObject<SymbolPriceTickerModel>(jsonSymbolPriceTickerData);

			if (symbolPriceTicker == null || string.IsNullOrWhiteSpace(symbolPriceTicker.Price))
			{
				TempData["Warning_Message"] = _localizer["FailedRetrievePriceMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}

			if (string.IsNullOrWhiteSpace(symbol) || convertedAmount <= 0)
			{
				TempData["Warning_Message"] = _localizer["InvalidDataProvidedMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				TempData["Warning_Message"] = _localizer["UserNotFoundMessage"].Value;
				return RedirectToAction("SignIn");
			}

			decimal coinPrice = decimal.Parse(symbolPriceTicker.Price, System.Globalization.CultureInfo.InvariantCulture);

			var userCash = await _dbContext.UserCash.FirstOrDefaultAsync(uc => uc.UserId == user.Id);
			if (userCash == null || userCash.Balance < (decimal)convertedAmount * coinPrice)
			{
				TempData["Warning_Message"] = _localizer["InsufficientBalanceMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}

			var transaction = new UserPortfolio
			{
				UserId = user.Id,
				Symbol = symbol,
				Amount = (decimal)convertedAmount,
				Price = coinPrice,
				TransactionDate = DateTime.UtcNow
			};

			_dbContext.UserPortfolio.Add(transaction);
			var transactionResult = await _dbContext.SaveChangesAsync();

			userCash.Balance -= (decimal)convertedAmount * coinPrice;
			userCash.LastUpdated = DateTime.UtcNow;

			_dbContext.UserCash.Update(userCash);
			var cashTransactionResult = await _dbContext.SaveChangesAsync();

			if (transactionResult > 0 && cashTransactionResult > 0)
			{
				TempData["Success_Message_"] = _localizer["TransactionSuccessfulMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}
			else
			{
				TempData["Warning_Message"] = _localizer["TransactionFailedMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}
		}

		[HttpPost("UserPortfolio/Sell")]
		public async Task<IActionResult> PortfolioSellCoin(string symbol, string amount)
		{
			double convertedAmount = System.Convert.ToDouble(amount, System.Globalization.CultureInfo.InvariantCulture);

			if (string.IsNullOrWhiteSpace(symbol) || convertedAmount <= 0)
			{
				TempData["Warning_Message"] = _localizer["InvalidDataProvidedMessage"].Value;
				return RedirectToAction("UserPortfolio");
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				TempData["Warning_Message"] = _localizer["UserNotFoundMessage"].Value;
				return RedirectToAction("SignIn");
			}

			var userPortfolio = await _dbContext.UserPortfolio
				.FirstOrDefaultAsync(up => up.UserId == user.Id && up.Symbol == symbol);

			if (userPortfolio == null)
			{
				TempData["Warning_Message"] = _localizer["InsufficientCoinBalanceinPortfolioForOrderMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}

			// Coin'in market değerini al
			var jsonSymbolPriceTickerData = await _market.SymbolPriceTicker(symbol);
			var symbolPriceTicker = JsonConvert.DeserializeObject<SymbolPriceTickerModel>(jsonSymbolPriceTickerData);

			if (symbolPriceTicker == null || string.IsNullOrWhiteSpace(symbolPriceTicker.Price))
			{
				TempData["Warning_Message"] = _localizer["FailedRetrievePriceMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}

			decimal coinPrice = decimal.Parse(symbolPriceTicker.Price, System.Globalization.CultureInfo.InvariantCulture);

			// Portföy güncelleme: coin miktarı düşürülür.
			userPortfolio.Amount -= (decimal)convertedAmount;

			if (userPortfolio.Amount <= 0)
			{
				_dbContext.UserPortfolio.Remove(userPortfolio); // Remove if balance is zero
			}
			else
			{
				_dbContext.UserPortfolio.Update(userPortfolio); // Update remaining balance
			}

			// Update user's cash balance
			var userCash = await _dbContext.UserCash.FirstOrDefaultAsync(uc => uc.UserId == user.Id);
			if (userCash == null)
			{
				TempData["Warning_Message"] = _localizer["CashAccountNotFoundMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}

			userCash.Balance += (decimal)convertedAmount * coinPrice;
			userCash.LastUpdated = DateTime.UtcNow;

			_dbContext.UserCash.Update(userCash);

			// Save changes to the database
			var portfolioTransactionResult = await _dbContext.SaveChangesAsync();

			if (portfolioTransactionResult > 0)
			{
				TempData["Success_Message_"] = _localizer["TransactionSuccessfulMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}
			else
			{
				TempData["Warning_Message"] = _localizer["TransactionFailedMessage"].Value;
				return RedirectToAction("UserPortfolio", "Member");
			}
		}

		[HttpGet("Margin")]
		public async Task<IActionResult> Margin()
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser == null)
			{
				return RedirectToAction("SignIn");
			}

			var userMarginCash = await _dbContext.UserMarginCash
				.FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

			var margins = _dbContext.UserMargins
				.Where(m => m.UserId == _userManager.GetUserId(User))
				.Select(m => new UserMarginViewModel
				{
					Id = m.Id,
					CoinSymbol = m.Symbol,
					EntryPrice = m.EntryPrice,
					ExitPrice = m.ExitPrice,
					OriginalAmount = m.OriginalAmount,
					BorrowAmount = m.BorrowAmount,
					IsLongMargin = m.IsLongMargin,
					IsCompleted = m.IsCompleted,

					// Kar-Zarar Hesaplaması:
					ProfitOrLoss = m.IsLongMargin
						? ((m.ExitPrice - m.EntryPrice) * m.OriginalAmount)
						: ((m.EntryPrice - m.ExitPrice) * m.OriginalAmount) // Short margin için ters hesap
				})
				.Take(50)
				.ToList();

			var coins = _dbContext.CoinList
				.Select(c => new CoinList
				{
					Id = c.Id,
					CoinName = c.CoinName
				})
				.ToList();

			ViewBag.Coins = new SelectList(coins, "CoinName", "CoinName");

			var viewModel = new MarginsPageViewModel
			{
				Margins = margins
			};

			ViewBag.UserMarginBalance = userMarginCash!.Balance;

			return View(viewModel);
		}

		[HttpPost("Margin")]
		public async Task<IActionResult> Margin(MarginsPageViewModel request)
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser == null)
			{
				return RedirectToAction("SignIn");
			}

			var userMarginCash = await _dbContext.UserMarginCash
				.FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

			var newMargin = new UserMargin
			{
				UserId = currentUser.Id,
				Symbol = request.NewMargin.CoinSymbol.ToUpper(),
				EntryPrice = (decimal)request.NewMargin.EntryPrice,
				ExitPrice = (decimal)request.NewMargin.ExitPrice,
				OriginalAmount = (decimal)request.NewMargin.OriginalAmount,		// alım: USDT tipi / satım: quantity tipi
				BorrowAmount = (decimal)request.NewMargin.BorrowAmount,
				IsLongMargin = request.NewMargin.IsLongMargin,
				CreatedDate = DateTime.Now,
				IsCompleted = false
			};

			decimal coinPrice = await GetCurrentPrice(newMargin.Symbol);

			// Alım mı? Long(Buy/Long) işlemi için :
			if (newMargin.IsLongMargin == true)
			{
                // Alımdaki kontroller : Kullanıcının yeterli bakiyesi yok veya portföyü yoksa işlem yapılamaz
                if (userMarginCash == null || userMarginCash!.Balance < newMargin.OriginalAmount)
				{
					TempData["NewMarginErrorMessage"] = _localizer["InsufficientBalanceForMarginMessage"].Value;
					return RedirectToAction("Margin");
				}
				else
				// Yeterli bakiyesi varsa
				{
					userMarginCash.Balance -= newMargin.OriginalAmount * coinPrice;
					
					_dbContext.UserMarginCash.Update(userMarginCash);
				}
			}
			// Satım? Short(Sell/Short) işlemi için: 
			else if (newMargin.IsLongMargin == false)
			{
				// Kullanıcı portföyünde coin'in olmasına gerek yok.
				// Burada OriginalAmount, satılacak coin miktarını ifade eder.
				// Gerekli teminat = coinPrice * OriginalAmount
				decimal requiredCollateral = coinPrice * newMargin.OriginalAmount;

				if (userMarginCash == null || userMarginCash!.Balance < requiredCollateral)
				{
					TempData["NewMarginErrorMessage"] = _localizer["InsufficientCollateralBalanceForMarginMessage"].Value;
					return RedirectToAction("Margin");
				}
				else
				// Yeterli teminat varsa
				{
					userMarginCash.Balance -= requiredCollateral;
					_dbContext.UserMarginCash.Update(userMarginCash);
				}
			}

			_dbContext.UserMargins.Add(newMargin);
			await _dbContext.SaveChangesAsync();

			TempData["MarginSuccessMessage"] = _localizer["MarginHasBeenCreatedSuccesfullyMessage"].Value;
			return RedirectToAction("Margin");
		}

		[HttpPost("DeleteMargin")]
		public async Task<IActionResult> DeleteMargin(int marginId)
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser == null)
			{
				return RedirectToAction("SignIn");
			}

			var userMarginCash = await _dbContext.UserMarginCash
				.FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

			var margin = await _dbContext.UserMargins
				.FirstOrDefaultAsync(m => m.Id == marginId && m.UserId == currentUser.Id);

			if (margin == null)
			{
				TempData["MarginDeleteErrorMessage"] = _localizer["MarginNotFoundorYoureNotAuthorizedToDeleteMessage"].Value;
				return RedirectToAction("Margin");
			}

			if (userMarginCash == null)
			{
				TempData["MarginDeleteErrorMessage"] = _localizer["UserMarginCashAccountNotFoundMessage"].Value;
				return RedirectToAction("Margin");
			}

			decimal currentPrice = await GetCurrentPrice(margin.Symbol);
			decimal profitLoss = 0;

            if (margin.IsLongMargin)
            {
                // Long işlem: (Original + Borrow) USDT ile coin alındı
                decimal coinAmount = (margin.OriginalAmount + margin.BorrowAmount);
                profitLoss = (currentPrice - margin.EntryPrice) * coinAmount;
				userMarginCash.Balance += (currentPrice * margin.OriginalAmount) + profitLoss;
			}
            else
            {
                // Short işlem: coin satışıyla (Original + Borrow) pozisyon açıldı
                decimal coinAmount = margin.OriginalAmount + margin.BorrowAmount;
                profitLoss = (margin.EntryPrice - currentPrice) * coinAmount;
                userMarginCash.Balance += (margin.OriginalAmount * currentPrice) + profitLoss;
            }

            await _dbContext.Database.ExecuteSqlRawAsync("EXEC sp_DeleteUserMargin {0}", marginId);
			await _dbContext.SaveChangesAsync();

			TempData["MarginDeleteSuccessMessage"] = _localizer["MarginHasBeenDeletedSuccessfullyMessage"].Value;
			return RedirectToAction("Margin");
		}


		[HttpGet("SendEmailForSupport")]
		public async Task<IActionResult> SendEmailForSupport()
		{
            var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

            if (currentUser == null)
            {
                return RedirectToAction("SignIn");
            }

            ViewBag.UserEmail = currentUser.Email;

            return View();
		}

        [HttpPost("SendEmailForSupport")]
        public async Task<IActionResult> SendEmailForSupport(SendEmailForSupportViewModel request)
        {
			var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (currentUser == null)
			{
                return RedirectToAction("SignIn", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("andunie123@gmail.com", "zngfrllrgxscwkaj"),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("andunie123@gmail.com", "CryptoSphere Support"),
                        To = { "Cryptospheree@hotmail.com" },
                        Subject = request.Subject,
                        Body = $"Gönderen Adı Soyadı: {request.Name} {request.Surname}\nGönderen Email: {request.Email}\n\nMesaj:\n{request.Message}",
                        IsBodyHtml = false,
                    };

                    // Kullanıcının emailini ReplyTo olarak eklemek iyi bir pratik
                    mailMessage.ReplyToList.Add(new MailAddress(request.Email));

                    await smtpClient.SendMailAsync(mailMessage);

                    ViewBag.Message = _localizer["EmailSendSuccessfullyMessage"].Value;
                    ModelState.Clear(); // Formu temizler
                    return View();
                }
                catch (Exception ex)
                {
                    ViewBag.Message = _localizer["EmailNotSendMessage"].Value + ex.Message;
                    return View(request);
                }
            }

            ViewBag.Message = _localizer["PleaseFillOutTheFormCompletelyMessage"].Value;
            return View(request);
        }

		[HttpGet("TransferCoin")]
		public async Task<IActionResult> TransferCoin()
		{
            var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

            if (currentUser == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            var coins = _dbContext.CoinList
                .Select(c => new CoinList
                {
                    Id = c.Id,
                    CoinName = c.CoinName
                })
                .ToList();

            ViewBag.Coins = new SelectList(coins, "CoinName", "CoinName");

            return View();
		}

        [HttpPost("TransferCoin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferCoin(TransferViewModel request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            if (!ModelState.IsValid)
            {
                var coins = await _dbContext.CoinList
                    .Select(c => new { c.CoinName })
                    .ToListAsync();
                ViewBag.Coins = new SelectList(coins, "CoinName", "CoinName");
                return View(request);
            }

            if (request.Amount <= 0)
            {
                ViewBag.ErrorMessage = _localizer["InvalidAmount"].Value;
                return View(request);
            }

            try
            {
                var sender = currentUser;
                var sendingCoin = await _dbContext.UserPortfolio
                    .Where(p => p.UserId == sender.Id && p.Symbol == request.SelectCoin)
                    .SumAsync(p => p.Amount);
                if (sendingCoin < request.Amount)
                {
                    ViewBag.ErrorMessage = _localizer["InsufficientCoinBalanceInPortfolioMessage"].Value;
                    return View(request);
                }

                var receiverMarginCashAddress = await _dbContext.UserMarginCash
                    .FirstOrDefaultAsync(c => c.WalletAddress == request.RecipientWalletAddress);
                var receiverUserCashAddress = await _dbContext.UserCash
                    .FirstOrDefaultAsync(c => c.WalletAddress == request.RecipientWalletAddress);
                if (receiverUserCashAddress == null && receiverMarginCashAddress == null)
                {
                    ViewBag.ErrorMessage = _localizer["ReceiverNotFound"].Value;
                    return View(request);
                }

                var coinPrice = await GetCurrentPrice(request.SelectCoin);
                if (coinPrice <= 0)
                {
                    ViewBag.ErrorMessage = _localizer["CoinPriceNotFound"].Value;
                    return View(request);
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Gönderici işlemi
                    var negativeTransfer = new UserPortfolio
                    {
                        UserId = sender.Id,
                        Symbol = request.SelectCoin,
                        Amount = -request.Amount,
                        Price = coinPrice,
                        TransactionDate = DateTime.UtcNow
                    };
                    _dbContext.UserPortfolio.Add(negativeTransfer);

                    // Alıcı işlemi
                    var receiverId = receiverUserCashAddress?.UserId ?? receiverMarginCashAddress!.UserId;

                    if (receiverUserCashAddress != null)
                    {
                        // Mevcut UserCash kaydını güncelle
                        receiverUserCashAddress.Balance += request.Amount * coinPrice;
                        receiverUserCashAddress.LastUpdated = DateTime.UtcNow;
                        _dbContext.UserCash.Update(receiverUserCashAddress);
                    }
                    else if (receiverMarginCashAddress != null)
                    {
                        // Mevcut UserMarginCash kaydını güncelle
                        receiverMarginCashAddress.Balance += request.Amount * coinPrice;
                        receiverMarginCashAddress.LastUpdated = DateTime.UtcNow;
                        _dbContext.UserMarginCash.Update(receiverMarginCashAddress);
                    }
                    else
                    {
                        // Yeni UserCash kaydı oluştur (nadir durum, mevcut kayıt varken gerekmeyebilir)
                        var positiveTransfer = new UserCash
                        {
                            UserId = receiverId,
                            Balance = request.Amount * coinPrice,
                            LastUpdated = DateTime.UtcNow,
                            WalletAddress = request.RecipientWalletAddress
                        };
                        _dbContext.UserCash.Add(positiveTransfer);
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = _localizer["TransferCompletedSuccessfully"].Value;
                    return RedirectToAction("TransferCoin");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Hata: {ex.Message}, İç Hata: {ex.InnerException?.Message}");
                    ViewBag.ErrorMessage = _localizer["AnErrorOccurred"].Value;
                    return View(request);
                }
            }
            catch (Exception ex) 
            {
				TempData["ErrorMessage"] = "Hata oluştu" + ex.Message;
                return View(request);
            }
        }

        [HttpGet("GetCoinPrice")]
        public async Task<IActionResult> GetCoinPrice(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return BadRequest("Symbol is required");

            var price = await GetCurrentPrice(symbol);
            return Ok(price);
        }

        private async Task<decimal> GetCurrentPrice(string symbol)
        {
            var jsonSymbolPriceTickerData = await _market.SymbolPriceTicker(symbol);
            var symbolPriceTicker = JsonConvert.DeserializeObject<SymbolPriceTickerModel>(jsonSymbolPriceTickerData);
            decimal coinPrice = decimal.Parse(symbolPriceTicker!.Price!, System.Globalization.CultureInfo.InvariantCulture);

            return coinPrice;
        }

    }
}