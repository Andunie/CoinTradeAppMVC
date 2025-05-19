using CoinTradeAppMVC.OptionsModels;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CoinTradeAppMVC.Services
{
	public class EmailService : IEmailService
	{
		private readonly EmailSettings _emailSettings;

		public EmailService(IOptions<EmailSettings> emailSettings)
		{
			_emailSettings = emailSettings.Value;
		}

		public async Task SendResetPasswordEmail(string resetPasswordEmailLink, string ToEmail)
		{
			var smptClient = new SmtpClient
			{
				Host = _emailSettings.Host,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Port = 587,
				Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password),
				EnableSsl = true
			};

			var mailMessage = new MailMessage();

			mailMessage.From = new MailAddress(_emailSettings.Email);
			mailMessage.To.Add(ToEmail);

			mailMessage.Subject = "CryptoSphere | Reset Password Link";
			mailMessage.Body = @$"<h4>To reset your password click on the link below </h4>
						  <p><a href='{resetPasswordEmailLink}'>Your Reset Password Link</a></p>";


			mailMessage.IsBodyHtml = true;

			await smptClient.SendMailAsync(mailMessage);
		}

		public async Task SendConfirmEmail(string confirmEmailLink, string ToEmail)
		{
			var smptClient = new SmtpClient
			{
				Host = _emailSettings.Host,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Port = 587,
				Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password),
				EnableSsl = true
			};

			var mailMessage = new MailMessage();

			mailMessage.From = new MailAddress(_emailSettings.Email);
			mailMessage.To.Add(ToEmail);

			mailMessage.Subject = "CryptoSphere | Confirm Your Email";
			mailMessage.Body = @$"<h4>To confirm your email click on the link below </h4>
						  <p><a href='{confirmEmailLink}'>Your Confirm Email Link</a></p>";

			mailMessage.IsBodyHtml = true;

			await smptClient.SendMailAsync(mailMessage);
		}
	}
}
