using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Delivery.Azure.Library.Configuration.Environments.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.Configuration.Initialization
{
    /// <summary>
	///     Contains methods which assist in runtime configuration based on .net core conventions
	/// </summary>
	public static class PlatformConfigurationInitialization
	{
		/// <summary>
		///     Sets up the minimal expected configuration by combining environment variables with the appsettings.json file.
		///     For local development the appsettings.Local.json overrides the appsettings.json settings.
		///     Environment-specific settings can be provided such as appsettings.prd.json (based on
		///     <see cref="RuntimeEnvironment" />) but environment variables and secrets
		///     are the preferred approach
		///     Dependencies:
		///     [None]
		///     Settings:
		///     [None]
		/// </summary>
		/// <param name="configurationBuilder">The configuration builder which combines different configuration sources</param>
		/// <param name="hostingEnvironment">
		///     Information about the runtime hosting environment such as the environment name
		/// </param>
		/// <param name="caller"></param>
		/// <returns>The configuration builder that can be extended by the caller</returns>
		public static IConfigurationBuilder UsePlatformConfiguration(this IConfigurationBuilder configurationBuilder, IHostEnvironment hostingEnvironment, [CallerFilePath] string? caller = null)
		{
			if (string.IsNullOrEmpty(caller))
			{
				throw new NotSupportedException("Expected to find a caller name");
			}

			return configurationBuilder.UsePlatformConfigurationCore(hostingEnvironment.EnvironmentName, caller);
		}

		/// <summary>
		///     Builds the <see cref="IConfigurationBuilder" /> and assigns this configuration to the
		///     <see cref="HostBuilderContext" />
		/// </summary>
		/// <param name="configurationBuilder">The configuration builder which combines different configuration sources</param>
		/// <param name="hostBuilderContext">The context which the hosted application is running in</param>
		public static IConfigurationBuilder Build(this IConfigurationBuilder configurationBuilder, HostBuilderContext hostBuilderContext)
		{
			hostBuilderContext.Configuration = configurationBuilder.Build();
			return configurationBuilder;
		}

		/// <summary>
		///     Tests are run from the tests project but call host libraries from the same project bin folder
		///     the appsettings.json files therefore need to be loaded from the original file locations during the test runs
		///     to ensure that each host is statically configured correctly
		///     this is not the case when running in containers or when hosts run in their own processes
		/// </summary>
		public static IConfigurationBuilder UseSimulationBasePath(this IConfigurationBuilder configurationBuilder, HostBuilderContext hostBuilderContext)
		{
			var isRunningInTestContext = Assembly.GetEntryAssembly()?.ManifestModule.Name.ToLowerInvariant() == "testhost.dll";
			if (isRunningInTestContext)
			{
				var targetAssembly = Assembly.GetCallingAssembly();
				var targetAssemblyName = targetAssembly.FullName!.Split(separator: ',').First();

				var binDirectorySearch = Directory.GetParent(Directory.GetCurrentDirectory());
				while (binDirectorySearch != null)
				{
					if (binDirectorySearch.Name.ToLowerInvariant() == "bin")
					{
						break;
					}

					binDirectorySearch = binDirectorySearch.Parent;
				}

				if (binDirectorySearch?.Parent == null)
				{
					throw new DirectoryNotFoundException($"Could not find a bin folder to locate the assembly {targetAssemblyName}");
				}

				var sourceAssemblyName = binDirectorySearch.Parent.Name;
				var assemblyDirectory = hostBuilderContext.HostingEnvironment.ContentRootPath.Replace(sourceAssemblyName, targetAssemblyName);
				configurationBuilder.SetBasePath(assemblyDirectory);
			}

			return configurationBuilder;
		}

		private static IConfigurationBuilder UsePlatformConfigurationCore(this IConfigurationBuilder configurationBuilder, string environmentName, string caller)
		{
			if (string.IsNullOrEmpty(environmentName))
			{
				throw new NotSupportedException("No environment has been set in the hosting environment. Ensure that it has been set as an environment variable");
			}

			var entryAssembly = Directory.GetParent(caller)?.FullName;
			if (entryAssembly == null)
			{
				throw new NotSupportedException("Could not determine the entry assembly");
			}

			configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
			if (!Enum.TryParse<RuntimeEnvironment>(environmentName, ignoreCase: true, out _))
			{
				throw new InvalidOperationException($"The {nameof(environmentName)} with value {environmentName} must be set in the ASPNETCORE_ENVIRONMENT environment variable matching one of the following: {string.Join(", ", Enum.GetNames(typeof(RuntimeEnvironment)))}");
			}

			var environmentSpecificJsonFile = $"appsettings.{environmentName.ToLowerInvariant()}.json";
			var entryJson = Path.Combine(entryAssembly, "appsettings.json");
			var entryEnvironmentJson = Path.Combine(entryAssembly, environmentSpecificJsonFile);

			configurationBuilder
				.AddJsonFile(environmentSpecificJsonFile, optional: true, reloadOnChange: false)
				.AddJsonFile(entryJson, optional: true, reloadOnChange: false)
				.AddJsonFile(entryEnvironmentJson, optional: true, reloadOnChange: false)
				.AddInMemoryCollection(new[] {new KeyValuePair<string, string>("ASPNETCORE_ENVIRONMENT", environmentName)})
				.AddEnvironmentVariables();

			return configurationBuilder;
		}
	}
}