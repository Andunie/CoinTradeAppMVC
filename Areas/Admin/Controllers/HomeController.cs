using Azure.Core;
using CoinTradeAppMVC.Areas.Admin.Models;
using CoinTradeAppMVC.ViewModels;
using CoinTradeAppMVC.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoinTradeAppMVC.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class HomeController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<AppRole> _roleManager;
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;

        public HomeController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, HttpClient httpClient, AppDbContext dbContext)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_httpClient = httpClient;
			_dbContext = dbContext;
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> UserList()
		{
			var userList = await _userManager.Users.ToListAsync();

			var userViewModelList = userList.Select(x => new CoinTradeAppMVC.Areas.Admin.Models.UserViewModel()
			{
				Id = x.Id,
				Email = x.Email,
				Name = x.UserName	
			}).ToList();

			return View(userViewModelList);
		}

		[HttpGet("AddBalance")]
        public async Task<IActionResult> AddBalance()
		{
            var userBalanceViewModel = (from cash in _dbContext.UserCash
                                        join user in _dbContext.Users
                                        on cash.UserId equals user.Id
                                        select new UserBalanceViewModel
                                        {
                                            UserId = cash.UserId,
                                            Balance = cash.Balance,
                                            UserName = user.UserName
                                        }).ToList();

            var viewModel = new BalancesPageViewModel
			{
				Balances = userBalanceViewModel
			};

            return View(viewModel);
		}

		[HttpPost("AddBalance")]
		public async Task<IActionResult> AddBalance(BalancesPageViewModel request)
		{
			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
				return BadRequest(new { message = "Model binding hatası", errors });
			}

			var user = await _userManager.FindByIdAsync(request.NewUserBalance.UserId!);
			if (user == null)
			{
				return NotFound("Kullanıcı bulunamadı.");
			}

			var userCash = await _dbContext.UserCash.FirstOrDefaultAsync(uc => uc.UserId == user.Id);
			if (userCash == null)
			{
				return NotFound("Kullanıcı bakiyesi bulunamadı.");
			}

			decimal amountToAdd = request.NewUserBalance.Balance;
			userCash.Balance += amountToAdd;
			await _dbContext.SaveChangesAsync();

			TempData["AddBalanceSuccessMessage"] = "The amount added to user's balance successfully!";
			return RedirectToAction("AddBalance");
		}

		[HttpGet("AddMarginBalance")]
		public async Task<IActionResult> AddMarginBalance()
		{
			var userMarginBalanceViewModel = (from marginCash in _dbContext.UserMarginCash
											  join user in _dbContext.Users
											  on marginCash.UserId equals user.Id
											  select new UserMarginBalanceViewModel
											  {
												  UserId = marginCash.UserId,
												  Balance = marginCash.Balance,
												  UserName = user.UserName
											  }).ToList();

			var viewModel = new MarginBalancesPageViewModel
			{
				MarginBalances = userMarginBalanceViewModel
			};

			return View(viewModel);
		}

		[HttpPost("AddMarginBalance")]
		public async Task<IActionResult> AddMarginBalance(MarginBalancesPageViewModel request)
		{
			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
				return BadRequest(new { message = "Model binding hatası", errors });
			}

			var user = await _userManager.FindByIdAsync(request.NewUserMarginBalance.UserId!);
			if (user == null)
			{
				return NotFound("Kullanıcı bulunamadı.");
			}

			var userMarginCash = await _dbContext.UserMarginCash.FirstOrDefaultAsync(umc => umc.UserId == user.Id);
			if (userMarginCash == null)
			{
				return NotFound("Kullanıcı bakiyesi bulunamadı.");
			}

			decimal amountToAdd = request.NewUserMarginBalance.Balance;
			userMarginCash.Balance += amountToAdd;
			await _dbContext.SaveChangesAsync();

			TempData["AddMarginBalanceSuccessMessage"] = "The amount added to user's margin balance successfully!";

			return RedirectToAction("AddMarginBalance");
		}
    }
}
