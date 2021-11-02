using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using SuperCRM.DataModels;
using SuperCRM.Models;
using SuperCRM.Repositories;

namespace SuperCRM.Controllers
{
	[Route("user")]
	[ApiController]
	public class UserController : SiteControllerBase
	{
		private readonly IAuthSessionProvider authSessionProvider;
		private readonly IUserPermitRepository permitRepository;
		public UserController(IUserService<Guid, Guid, DbUser> userService, INetSecuritySettings securitySettings,
			ISecurityUtility securityUtility, IConfig config, IAuthSessionProvider authSessionProvider, IUserPermitRepository permitRepository) : base(userService, securitySettings, securityUtility,
			config)
		{
			this.authSessionProvider = authSessionProvider;
			this.permitRepository = permitRepository;
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("sign-up")]
		public async Task<BaseResponse> SignUp(SignUp model)
		{
			if (ModelState.IsValid)
			{
				var dbUser = await this.UserService.NewUserAsync(model.Email, model.Password, model.Name);

				dbUser.Id = Guid.NewGuid();
				if (model.Type == AccountType.Team)
					dbUser.BusinessDetails = new DbBusinessDetails { Name = model.BusinessName };

				if (await this.UserService.CreateAccountAsync(dbUser))
				{
					await this.permitRepository.AddPermitAsync(dbUser.Id, "Customer", dbUser.Id);
					await SendVerificationMailAsync(dbUser);
					var result = await this.authSessionProvider.LoginAsync(model.Email, model.Password, false, this.SecuritySettings.LetSuspendedAuthenticate);

					return Ok(PopulateCurrentUserDetails(result));
				}

				return Error(AppOpResult.UsernameAlreadyExists, "An account with this email is already registered.");
			}

			return Error();
		}


		private async Task SendVerificationMailAsync(DbUser user)
		{
			// to use Gmail, you need to enable "Less secure app access" etc. for more information, visit https://support.google.com/a/answer/176600?hl=en#zippy=%2Cuse-the-restricted-gmail-smtp-server%2Cuse-the-gmail-smtp-server
			var username = "<YourGMailSmtpUsername>";
			var password = "<YourGMailSmtpPassword>";
			var host = "smtp.gmail.com";
			var verificationUrl = $"<verificationUrl>/{user.VerificationToken}";

			var mail = new MailMessage { From = new MailAddress(username) };
			mail.To.Add(user.Username);
			mail.Subject = "Verify your email";
			mail.Body = $@"<p>Hi {user.Name},<br/>Please click the link below to verify your email.<br/><a href='{verificationUrl}'>{verificationUrl}</a><br/>Thank you!</p>";
			mail.IsBodyHtml = true;

			var smtp = new SmtpClient(host, 587)
			{
				Credentials = new NetworkCredential(username, password),
				EnableSsl = true
			};

			await smtp.SendMailAsync(mail).ConfigureAwait(false);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("sign-in")]
		public async Task<BaseResponse> SignIn(SignIn model)
		{
			if (ModelState.IsValid)
			{
				var result = await this.authSessionProvider.LoginAsync(model.Email, model.Password, model.RememberMe, this.SecuritySettings.LetSuspendedAuthenticate);
				switch (result.Result)
				{
					case OpResult.Success:
						return Ok(PopulateCurrentUserDetails(result));
					case OpResult.Suspended:
						return Error(result.Result, "This account is suspended.");
					case OpResult.PasswordBlocked:
						return Error(result.Result, "Your password is blocked. Please reset the password using the 'forgot password' option.");
					default:
						return Error(OpResult.InvalidInput, "The email or password provided is incorrect.");
				}
			}

			return Error();
		}

		[HttpPost]
		[Feature(RequestFeature.AuthorizationNotRequired, RequestFeature.MFANotRequired)]
		[Route("sign-out")]
		public async Task<BaseResponse> SignOut()
		{
			await this.authSessionProvider.LogoutAsync();
			return Ok();
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("verify")]
		public async Task<BaseResponse> Verify(Verify model)
		{
			if (ModelState.IsValid)
			{
				switch (await this.UserService.VerifyAccountAsync(model.Token))
				{
					case OpResult.Success:
						return Ok();
					case OpResult.AlreadyDone:
						return Error(OpResult.AlreadyDone, "Account already verified");
					default:
						return Error(AppOpResult.InvalidToken, "Verification was not successful; please try again.");
				}
			}

			return Error();
		}

		[HttpPost]
		[VerificationNotRequired, SkipActivityAuthorization]
		[Route("self/verification-email")]
		public async Task<BaseResponse> ResendVerificationEmail()
		{
			if (this.UserService.IsAuthenticated && this.UserService.IsVerified)
			{
				return Error(OpResult.AlreadyDone, "Account already verified");
			}

			await SendVerificationMailAsync(this.UserService.CurrentUser);
			return Ok();
		}
	}
}
