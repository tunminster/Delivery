using System;
using Delivery.Azure.Library.Microservices.Hosting.Enums;
using Delivery.Azure.Library.Microservices.Hosting.Hosts;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.Microservices.Hosting.Extensions
{
    public static class PlatformHostingExtensions
    {
        public static IHostBuilder ConfigurePlatformHosting(this IHostBuilder hostBuilder, HostTypes hostTypes, Func<IHostBuilder, ContainerHost>? containerHostFactory = null)
        {
            hostBuilder.Properties[nameof(HostTypes)] = hostTypes.ToString();

            if (containerHostFactory != null && !hostTypes.HasFlag(HostTypes.MessagingHost))
            {
                throw new InvalidOperationException($"Host builder with type {hostTypes} needs to provide a factory to create the {nameof(ContainerHost)}");
            }

            if (containerHostFactory != null)
            {
                hostBuilder.Properties[nameof(ContainerHost)] = containerHostFactory.Invoke(hostBuilder);
            }

            return hostBuilder;
        }
    }
}