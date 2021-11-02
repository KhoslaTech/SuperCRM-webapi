using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using SuperCRM.DataModels;
using SuperCRM.Models;

namespace SuperCRM.Controllers
{
	[Route("user")]
	[ApiController]
	public class UserController : SiteControllerBase
	{
		private readonly IAuthSessionProvider authSessionProvider;

		public UserController(IUserService<Guid, Guid, DbUser> userService, INetSecuritySettings securitySettings,
			ISecurityUtility securityUtility, IConfig config, IAuthSessionProvider authSessionProvider) : base(userService, securitySettings, securityUtility,
			config)
		{
			this.authSessionProvider = authSessionProvider;
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
					var result = await this.authSessionProvider.LoginAsync(model.Email, model.Password, false, this.SecuritySettings.LetSuspendedAuthenticate);

					return Ok(PopulateCurrentUserDetails(result));
				}

				return Error(AppOpResult.UsernameAlreadyExists, "An account with this email is already registered.");
			}

			return Error();
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
	}
}
