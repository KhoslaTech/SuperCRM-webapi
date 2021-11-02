﻿using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SuperCRM.ModelBinding
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class FromBodyAndRouteAttribute : Attribute, IBindingSourceMetadata
	{
		public BindingSource BindingSource => BodyAndRouteBindingSource.BodyAndRoute;
	}
}