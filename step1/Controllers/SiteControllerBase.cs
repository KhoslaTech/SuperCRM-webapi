using SuperCRM.DataModels;
using SuperCRM.Models;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace SuperCRM.Controllers
{
	[ApiController]
	public class SiteControllerBase : ControllerBase
	{
		protected readonly IUserService<Guid, Guid, DbUser> UserService;
		protected readonly INetSecuritySettings SecuritySettings;
		protected readonly ISecurityUtility SecurityUtility;
		protected readonly IConfig Config;

		public SiteControllerBase(IUserService<Guid, Guid, DbUser> userService,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, IConfig config)
		{
			this.UserService = userService;
			this.SecuritySettings = securitySettings;
			this.SecurityUtility = securityUtility;
			this.Config = config;
		}

		protected AppUserDetails PopulateCurrentUserDetails(LoginResult result = null)
		{
			var userDetails = new AppUserDetails
			{
				Id = this.UserService.CurrentUser.Id,
				Name = this.UserService.CurrentUser.Name,
				Username = this.UserService.CurrentUser.Username,
				SessionId = result != null ? IdentityTokenType.GetToken(result.Auth.AuthUrn) : null,
				Secret = result?.Auth.Secret,
				VerificationRequired = this.SecuritySettings.MustHaveBeenVerified && !this.UserService.CurrentUser.Verified,
				CreatedDate = this.UserService.CurrentUser.CreatedDate
			};

			return userDetails;
		}

		protected new BaseResponse Ok() => new BaseResponse();

		protected BaseIdResponse Ok(Guid id) => new BaseIdResponse { Id = id };

		protected BaseRecordResponse<T> Ok<T>(T record) => new BaseRecordResponse<T> { Record = record };

		protected BaseListResponse<T> Ok<T>(IList<T> records) => new BaseListResponse<T> { Records = records };

		protected BaseListResponse<T> Ok<T>(List<T> records) => new BaseListResponse<T> { Records = records };

		protected BaseListResponse<T> Ok<T>(List<T> records, long totalCount) => new BaseListResponse<T> { Records = records, TotalCount = totalCount };

		protected BaseListResponse<T> Ok<T>(PagedResult<T> result) => new BaseListResponse<T> { Records = result.Records, TotalCount = result.TotalCount };

		protected BaseResponse Error(OpResult code) => Error(code, string.Empty);

		protected BaseResponse Error(OpResult code, string message)
		{
			return BaseResponse.Error(code, this.SecurityUtility.GetErrorMessage(code, message, this.SecuritySettings.IsDevelopmentEnvironment));
		}

		protected BaseResponse Error()
		{
			var response = BaseResponse.Error(OpResult.InvalidInput, "Operation failed with validation issues");

			foreach (var modelState in ModelState)
			{
				if (!string.IsNullOrWhiteSpace(modelState.Key))
				{
					foreach (var err in modelState.Value.Errors)
					{
						response.ErrorDetail.Errors.Add(new FieldError
						{
							FieldName = modelState.Key,
							Message = !string.IsNullOrWhiteSpace(err.ErrorMessage)
								? err.ErrorMessage
								: err.Exception?.Message
						});
					}
				}
			}

			return response;
		}
	}
}