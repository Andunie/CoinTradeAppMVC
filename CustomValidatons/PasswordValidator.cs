using CoinTradeAppMVC.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace CoinTradeAppMVC.CustomValidatons
{
    public class PasswordValidator : IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
        {
            var errors = new List<IdentityError>();
            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                errors.Add(new() { Code = "PasswordNoContainUserName", Description = "Password can not contain username." });
            }

            if (password.ToLower().StartsWith("1234"))
            {
                errors.Add(new() { Code = "PasswordNoContain1234", Description = "Password can not contain consecutive numbers." });
            }

            if (!errors.Any())
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
