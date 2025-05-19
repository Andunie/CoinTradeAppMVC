namespace CoinTradeAppMVC.Services
{
	public interface IEmailService
	{
		Task SendResetPasswordEmail(string resetPasswordEmailLink, string ToEmail);

		Task SendConfirmEmail(string confirmEmailLink, string ToEmail);
	}
}
