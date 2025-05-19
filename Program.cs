using CoinTradeAppMVC.CustomValidatons;
using CoinTradeAppMVC.OptionsModels;
using CoinTradeAppMVC.Services;
using CoinTradeAppMVC.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Binance.Spot;
using CoinTradeAppMVC.Hubs;
using System.Configuration;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Runtime.Versioning;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Configuration.AddJsonFile("secret.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews()
	.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
	.AddDataAnnotationsLocalization();
builder.Services.AddHttpClient();

builder.Services.AddCors(opt =>
{
	opt.AddPolicy("CorsPolicy", builder =>
	{
		builder.AllowAnyHeader()
		.AllowAnyMethod()
		.SetIsOriginAllowed(host => true)
		.AllowCredentials();
	});
});
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("SqlCon"));
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
	options.ValidationInterval = TimeSpan.FromMinutes(30);
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOrderTradeService, OrderTradeService>();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
	options.TokenLifespan = TimeSpan.FromHours(2);
});

builder.Services.AddAuthentication().AddGoogle(opt =>
{
	opt.ClientId = builder.Configuration["Authentication:Google:ClientID"];
	opt.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
	opt.Scope.Add("email");
	opt.Scope.Add("profile");
	opt.SaveTokens = true; // Token'ý kaydetmek için
});

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
	options.User.RequireUniqueEmail = true;
	options.User.AllowedUserNameCharacters = "abcdefghijklmnoprstuvwxyz1234567890_";

	options.Password.RequiredLength = 6;
	options.Password.RequireDigit = false;

	// 5 Kere denemede 5 dakikalýðýna kitleme
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.MaxFailedAccessAttempts = 5;

})
.AddUserValidator<UserValidator>()
//.AddPasswordValidator<PasswordValidator>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpClient();

builder.Services.ConfigureApplicationCookie(options =>
{
	var cookieBuilder = new CookieBuilder();

	cookieBuilder.Name = "CoinTradeAppCookie";
	options.LoginPath = new PathString("/Home/SignIn");
	options.LogoutPath = new PathString("/Member/Logout");
	options.Cookie = cookieBuilder;

	// 30 gün boyunca cookie tutma
	options.ExpireTimeSpan = TimeSpan.FromDays(30);
	options.SlidingExpiration = true;
});

builder.Services.AddSingleton<Market>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
	var options = new ConfigurationOptions
	{
		EndPoints = { "localhost:6379" },
		AbortOnConnectFail = false, // Baðlantý baþarýsýz olsa da yeniden denemeye devam eder.
	};
	return ConnectionMultiplexer.Connect(options);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

// Dil Desteði kodlarý
var supportedCultures = new[] { "en-US", "tr-TR"};



var localizationOptions = new RequestLocalizationOptions()
	.SetDefaultCulture(supportedCultures[0])
	.AddSupportedCultures(supportedCultures)
	.AddSupportedUICultures(supportedCultures);

CultureInfo enCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = enCulture;
CultureInfo.DefaultThreadCurrentUICulture = enCulture;

app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("CorsPolicy");

app.MapControllerRoute(
	name: "areas",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Home}/{id?}");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Member}/{action=Index}/{id?}");

app.MapHub<CoinHub>("/coinhub");

app.Run();