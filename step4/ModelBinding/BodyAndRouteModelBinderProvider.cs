using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace SuperCRM.ModelBinding
{
	public class BodyAndRouteModelBinderProvider : IModelBinderProvider
	{
		private readonly BodyModelBinderProvider bodyModelBinderProvider;

		public BodyAndRouteModelBinderProvider(BodyModelBinderProvider bodyModelBinderProvider)
		{
			this.bodyModelBinderProvider = bodyModelBinderProvider;
		}

		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			var bodyBinder = this.bodyModelBinderProvider.GetBinder(context);

			if (context.BindingInfo.BindingSource != null
				&& context.BindingInfo.BindingSource.CanAcceptDataFrom(BodyAndRouteBindingSource.BodyAndRoute))
			{
				return new BodyAndRouteModelBinder(bodyBinder);
			}

			return null;
		}
	}
}