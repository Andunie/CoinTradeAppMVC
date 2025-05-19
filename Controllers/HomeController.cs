using Binance.Spot;
using CoinTradeAppMVC.ApiModels;
using CoinTradeAppMVC.Models;
using CoinTradeAppMVC.Services;
using CoinTradeAppMVC.ViewModels;
using CoinTradeAppMVC.Web.Models;
using CoinTradeAppMVC.Web.ViewModels;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Claims;
using CoinTradeAppMVC.Entities;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace CoinTradeAppMVC.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IEmailService _emailService;
		private readonly Market _market;
		private readonly AppDbContext _dbContext;
		private readonly IStringLocalizer<HomeController> _localizer;

		public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService,
			Market market, AppDbContext dbContext, IStringLocalizer<HomeController> localizer)
		{
			_logger = logger;
			_userManager = userManager;
			_signInManager = signInManager;
			_emailService = emailService;
			_market = market;
			_dbContext = dbContext;
			_localizer=localizer;
		}

		[HttpGet]
		public IActionResult SetLanguage(string culture, string returnUrl = null)
		{
			Response.Cookies.Append(
				CookieRequestCultureProvider.DefaultCookieName,
				CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
				new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
			);

			return LocalRedirect(returnUrl ?? Url.Action("Home", "Home")!);
		}

		public async Task<IActionResult> Home()
		{
			var coins = new List<HomeCoinViewModel>
			{
				new HomeCoinViewModel { Symbol = "BTC", Name = "Bitcoin", LogoUrl = "https://cdn.jsdelivr.net/gh/simplr-sh/coin-logos/images/bitcoin/large.png" },
				new HomeCoinViewModel { Symbol = "ETH", Name = "Ethereum", LogoUrl = "https://cdn.jsdelivr.net/gh/simplr-sh/coin-logos/images/ethereum/large.png" },
				new HomeCoinViewModel { Symbol = "BNB", Name = "BNB", LogoUrl = "https://cdn.jsdelivr.net/gh/simplr-sh/coin-logos/images/binancecoin/large.png" },
				new HomeCoinViewModel { Symbol = "XRP", Name = "XRP", LogoUrl = "https://cdn.jsdelivr.net/gh/simplr-sh/coin-logos/images/ripple/large.png" }
			};

			foreach (var coin in coins)
			{
				string jsonResponse = await _market.CurrentAveragePrice($"{coin.Symbol.ToUpper()}USDT");
				var currentPriceData = JsonConvert.DeserializeObject<CurrentAveragePriceModel>(jsonResponse);
				coin.Price = currentPriceData!.Price;

				string symbol = $"{coin.Symbol.ToUpper()}USDT";
				string priceChangePercent = await GetTicker(symbol);

				// Güvenli bir þekilde decimal'e çevirme
				if (decimal.TryParse(priceChangePercent, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal changePercent))
				{
					coin.ChangePercent = changePercent / 1000;
				}
				else
				{
					_logger.LogWarning("Invalid PriceChangePercent value for {Symbol}: {Value}", symbol, priceChangePercent);
					coin.ChangePercent = 0; // Varsayýlan bir deðer atayýn veya hata fýrlatýn
				}
			}

			return View(coins);
		}

		public IActionResult SignUp()
		{
			return View();
		}


		[HttpPost]
		public async Task<IActionResult> SignUp(SignUpViewModel request)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			var user = new AppUser
			{
				UserName = request.UserName.ToLower(),
				Email = request.Email,
				PhoneNumber = request.Phone
			};

			var identityResult = await _userManager.CreateAsync(user, request.PasswordConfirm);

			if (identityResult.Succeeded)
			{
				var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

				var confirmationLink = Url.Action("ConfirmEmail", "Home", new
				{
					userId = user.Id,
					Token = emailConfirmToken
				}, HttpContext.Request.Scheme);

				await _emailService.SendConfirmEmail(confirmationLink, user.Email);

				using (var transaction = await _dbContext.Database.BeginTransactionAsync())
				{
					try
					{
						var userCash = new UserCash
						{
							UserId = user.Id,
							Balance = 0.00m, // Varsayýlan nakit deðeri
							LastUpdated = DateTime.UtcNow,
                            WalletAddress = Guid.NewGuid().ToString("N")
                        };

						var userMarginCash = new UserMarginCash
						{
							UserId = user.Id,
							Balance = 0.00m, // Varsayýlan nakit deðeri
							LastUpdated = DateTime.UtcNow,
							WalletAddress = Guid.NewGuid().ToString("N")
						};

						_dbContext.UserCash.Add(userCash);
						_dbContext.UserMarginCash.Add(userMarginCash);
						await _dbContext.SaveChangesAsync();

						await transaction.CommitAsync();
					}
					catch (Exception ex)
					{
						// Eðer bir hata olursa, iþlemi geri al.
						await transaction.RollbackAsync();
						ModelState.AddModelError(string.Empty, "An error occurred while creating the cash record: " + ex.Message);
						return View();
					}
				}

				TempData["SuccessMessage"] = _localizer["SuccessMessage"].Value;
				return RedirectToAction(nameof(HomeController.SignUp));
			}

			foreach (IdentityError item in identityResult.Errors)
			{
				ModelState.AddModelError(string.Empty, item.Description);
			}

			return View();
		}

		public IActionResult SignIn()
        {
            return View();
        }

		[HttpPost]
		public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
		{
			returnUrl ??= Url.Action("Index", "Member");

			var hasUser = await _userManager.FindByEmailAsync(model.Email);

			if (hasUser == null)
			{
				ModelState.AddModelError(string.Empty, _localizer["EmailorPasswordWrongMessage"].Value);
				return View();
			}

			if (!hasUser.EmailConfirmed)
			{
				ModelState.AddModelError(string.Empty, _localizer["CanNotLoginWithoutConfirmEmailMessage"].Value);
				return View();
			}

			var signInResult = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);

			if (signInResult.Succeeded)
			{
				return Redirect(returnUrl);
			}

			if (signInResult.IsLockedOut)
			{
				ModelState.AddModelError(string.Empty, _localizer["YourLoginStatueisNotAllowedFor5MinMessage"].Value);
				return View();
			}

			ModelState.AddModelError(string.Empty, _localizer["EmailorPasswordWrongMessage"].Value);

            return View();
		}

		public IActionResult ForgetPassword()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel request)
		{
			var hasUser = await _userManager.FindByEmailAsync(request.Email);

			if (hasUser == null)
			{
				ModelState.AddModelError(string.Empty, _localizer["UserNotFoundByEmailMessage"].Value);
				return View();
			}

			string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);

			var passwordResetLink = Url.Action("ResetPassword", "Home", new {userId = hasUser.Id, Token = 
				passwordResetToken }, HttpContext.Request.Scheme);

			await _emailService.SendResetPasswordEmail(passwordResetLink, hasUser.Email);


			TempData["SuccessMessage"] = _localizer["PasswordResetMessage"].Value;

			return RedirectToAction(nameof(ForgetPassword));
		}

		public IActionResult ResetPassword(string userId, string token)
		{
			TempData["userId"] = userId;
			TempData["token"] = token;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
		{
			var userId = TempData["userId"];
			var token = TempData["token"];

			if (userId == null || token == null)
			{
				throw new Exception("Bir hata meydana geldi");
			}

			var hasUser = await _userManager.FindByIdAsync(userId.ToString()!);

			if (hasUser == null)
			{
				ModelState.AddModelError(string.Empty, _localizer["UserNotFoundMessage"].Value);
				return View();
			}

			IdentityResult result = await _userManager.ResetPasswordAsync(hasUser, token.ToString()!, request.Password);

			if (result.Succeeded)
			{
				TempData["SuccessMessage"] = _localizer["PasswordChangedSuccessfullyMessage"].Value;
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View();
		}


		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public async Task<string> GetTicker(string symbol)
		{
			var client = new HttpClient();
			var url = $"https://api.binance.com/api/v3/ticker?symbol={symbol}";

			var response = await client.GetAsync(url);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				_logger.LogInformation("Binance API Response for {Symbol}: {Response}", symbol, content); // API yanýtýný logla
				var tickerResponse = JsonConvert.DeserializeObject<TickerResponse>(content);

				if (tickerResponse?.PriceChangePercent == null)
				{
					throw new Exception($"PriceChangePercent is null for symbol: {symbol}");
				}

				return tickerResponse.PriceChangePercent;
			}
			else
			{
				throw new Exception($"Binance API error: {response.StatusCode}");
			}
		}

		public async Task<IActionResult> GoogleResponse()
		{
			var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

			if (!result.Succeeded)
			{
				var errorMessage = result.Failure?.Message ?? "Authentication failed.";
				_logger.LogError($"Google authentication failed: {errorMessage}");
				return RedirectToAction("Error", "Home");
			}

			// AccessToken'ý al
			var accessToken = result.Properties?.GetTokenValue("access_token");

			if (string.IsNullOrEmpty(accessToken))
			{
				// Eðer access token bulunamazsa hata sayfasýna yönlendir
				return RedirectToAction("Error", "Home");
			}

			// Google People API'ye istek yaparak kullanýcý verilerini al
			var userInfo = await GetGoogleUserInfoAsync(accessToken);

			if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
			{
				// Google'dan gelen bilgiler eksikse hata sayfasýna yönlendir
				return RedirectToAction("Error", "Home");
			}

		// Kullanýcýyý bul
		var user = await _userManager.FindByEmailAsync(userInfo.Email);

			if (user == null)
			{
				// Kullanýcý yoksa, yeni kullanýcý oluþtur
				user = new AppUser
				{
					UserName = userInfo.Email.Split('@')[0],
					Email = userInfo.Email,
					EmailConfirmed = true, // Google ile doðrulanan kullanýcýlar için e-posta onayý
				};

				var createResult = await _userManager.CreateAsync(user);

				if (createResult.Succeeded)
				{
					using (var transaction = await _dbContext.Database.BeginTransactionAsync())
					{
						try
						{
							var userCash = new UserCash
							{
								UserId = user.Id,
								Balance = 0.00m,
								LastUpdated = DateTime.UtcNow
							};

							_dbContext.UserCash.Add(userCash);
							await _dbContext.SaveChangesAsync();

							await transaction.CommitAsync();
						}
						catch (Exception ex)
						{
							await transaction.RollbackAsync();
							ModelState.AddModelError(string.Empty, "An  error occurred while creating the cash record: " + ex.Message);
							return RedirectToAction("Error", "Home");
						}
					}
				}
				else if (!createResult.Succeeded)
				{
					return RedirectToAction("Error", "Home");
				}
			}

			// Kullanýcýyý oturum açtýr
			await _signInManager.SignInAsync(user, isPersistent: false);

			// ReturnUrl'i al ve kullanýcýyý yönlendir
			var returnUrl = result.Properties?.Items["returnUrl"] ?? "/";
			return LocalRedirect(returnUrl);
		}

		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
			{
				TempData["ErrorMessage"] = _localizer["InvalidEmailConfirmationRequestMessage"].Value;
				return RedirectToAction("Index", "Home");
			}

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				TempData["ErrorMessage"] = _localizer["UserNotFoundMessage"].Value;
				return RedirectToAction("Index", "Home");
			}

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				TempData["SuccessMessage"] = _localizer["EmailConfirmedSuccessfullyMessage"].Value;
				return RedirectToAction("SignIn", "Home"); // Kullanýcýyý giriþ sayfasýna yönlendir
			}
			else
			{
				TempData["ErrorMessage"] = _localizer["EmailConfirmationFailedMessage"].Value;
				return RedirectToAction("Index", "Home");
			}
		}

		public IActionResult GoogleLogin(string returnUrl = "/")
		{
			// AuthenticationProperties ile geri dönüþ URL'sini ayarlayýn
			var properties = new AuthenticationProperties
			{
				RedirectUri = Url.Action("GoogleResponse"), // Google oturum açma tamamlandýðýnda çaðrýlacak yöntem
				Items = { { "returnUrl", returnUrl } } // ReturnUrl'i saklayýn
			};

			// Google kimlik doðrulama þemasý ile oturum açma iþlemini baþlatýn
			return Challenge(properties, GoogleDefaults.AuthenticationScheme);
		}

		private async Task<GoogleUserInfoModel> GetGoogleUserInfoAsync(string accessToken)
		{
			using (var client = new HttpClient())
			{
				// Google People API'ye istek gönder
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
				var response = await client.GetStringAsync("https://www.googleapis.com/oauth2/v3/userinfo");

				if (string.IsNullOrEmpty(response))
				{
					return null;
				}

				// Gelen JSON yanýtýný deserialize et
				return JsonConvert.DeserializeObject<GoogleUserInfoModel>(response);
			}
		}
	}
}
