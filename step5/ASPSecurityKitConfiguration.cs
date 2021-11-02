using SuperCRM.DataModels;
using SuperCRM.DependencyInjection;
using SuperCRM.Middlewares;
using SuperCRM.ModelBinding;
using ASPSecurityKit;
using ASPSecurityKit.NetCore;
using ASPSecurityKit.NetCore.Middleware;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using AutoMapper.Contrib.Autofac.DependencyInjection;

namespace SuperCRM
{
	public class ASPSecurityKitConfiguration
	{
		public static bool IsDevelopmentEnvironment { get; set; }

		public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.AddControllers(options =>
			{
				options.Filters.Add(typeof(ProtectAttribute));
			})
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
					options.JsonSerializerOptions.IgnoreNullValues = true;
				})
				.AddBodyAndRouteModelBinder();

			services.AddDbContext<AppDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

			services.AddHttpContextAccessor();
		}

		public static void ConfigureContainer(ContainerBuilder builder)
		{
			License.TryRegisterFromExecutionPath();

			// Register all ASK components and auth definitions
			new ASPSecurityKitRegistry()
				.Register(new ASKContainerBuilder(builder));

			builder.RegisterModule<AppRegistry>();
			builder.RegisterAutoMapper(typeof(WebAppProfile).Assembly);
		}

		public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			IsDevelopmentEnvironment = env.IsDevelopment();

			app.UseRequestBuffering();
			app.UseAuthSessionCaching();

		}

	}
}