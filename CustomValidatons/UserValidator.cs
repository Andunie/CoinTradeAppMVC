using CoinTradeAppMVC.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace CoinTradeAppMVC.CustomValidatons
{
    public class UserValidator : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            var errors = new List<IdentityError>();

            var isDigit = int.TryParse(user.UserName![0].ToString(), out _);

            if (isDigit)
            {
                errors.Add(new() { Code = "UserNameContainFirstLetterDigit", Description = "User name's first letter do not contain digits." });
            }

            if (errors.Any())
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
