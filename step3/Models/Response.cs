using System;
using System.Collections.Generic;

namespace SuperCRM.Models
{
	public class FieldError
	{
		public string ErrorCode { get; set; }

		public string FieldName { get; set; }

		public string Message { get; set; }

		public Dictionary<string, string> Meta { get; set; }
	}

	public class ResponseError
	{
		public ResponseError(string errorCode) : this(errorCode, string.Empty)
		{
		}

		public ResponseError(string errorCode, string message)
		{
			this.ErrorCode = errorCode;
			this.Message = message;
			this.Errors = new List<FieldError>();
		}

		public string ErrorCode { get; set; }

		public string Message { get; set; }

		public List<FieldError> Errors { get; set; }
	}
	
	public class BaseResponse
	{
		public bool Succeeded => ErrorDetail == null;

		public ResponseError ErrorDetail { get; set; }

		public static BaseResponse Error(string code)
		{
			return new BaseResponse { ErrorDetail = new ResponseError(code) };
		}

		public static BaseResponse Error(string code, string message)
		{
			return new BaseResponse { ErrorDetail = new ResponseError(code, message) };
		}
	}

	public class BaseRecordResponse<T> : BaseResponse
	{
		public T Record { get; set; }
	}

	public class BaseListResponse<T> : BaseResponse
	{
		public IList<T> Records { get; set; }

		private long? totalCount;
		public long? TotalCount
		{
			get =>
				totalCount == null && Records != null
					? Records.Count
					: totalCount;
			set => totalCount = value;
		}
	}

	public class BaseIdResponse : BaseResponse
	{
		public Guid Id { get; set; }
	}
}