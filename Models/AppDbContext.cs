using CoinTradeAppMVC.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoinTradeAppMVC.Web.Models
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserPortfolio> UserPortfolio { get; set; }

        public DbSet<UserCash> UserCash { get; set; }

        public DbSet<UserOrders> UserOrders { get; set; }

        public DbSet<UserMargin> UserMargins { get; set; }

        public DbSet<UserMarginCash> UserMarginCash { get; set; }

        public DbSet<CoinList> CoinList { get; set; }
    }
}
