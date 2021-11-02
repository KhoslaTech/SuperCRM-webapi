using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;

namespace SuperCRM.ModelBinding
{
	public static class ModelBinderExtensions
	{
		public static IMvcBuilder AddBodyAndRouteModelBinder(this IMvcBuilder builder)
		{
			builder = builder ?? throw new ArgumentNullException(nameof(builder));

			builder.AddMvcOptions(options =>
			{
				var bodyModelBinderProvider = options.ModelBinderProviders.Single(x => x.GetType() == typeof(BodyModelBinderProvider)) as
						BodyModelBinderProvider;
				var bodyAndRouteProvider = new BodyAndRouteModelBinderProvider(bodyModelBinderProvider);
				options.ModelBinderProviders.Insert(0, bodyAndRouteProvider);
			});

			return builder;
		}
	}
}