using System;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Initializers.Custom
{
    public class ApplicationMapTelemetryInitializer : TelemetryInitializer
    {
        private const string NodeNameSettingName = "Node_Name";
        private const string InstanceNameSettingName = "Instance_Name";
        private const string ServiceNameSettingName = "Service_Name";

        public ApplicationMapTelemetryInitializer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void OnInitialize(ITelemetry telemetry)
        {
            var configurationProvider = ServiceProvider.GetRequiredService<IConfigurationProvider>();

            var ring = configurationProvider.GetSetting("Ring", isMandatory: false);

            var nodeName = configurationProvider.GetSetting(NodeNameSettingName, isMandatory: false);
            telemetry.Context.GlobalProperties[NodeNameSettingName.Replace("_", string.Empty)] = nodeName;

            var serviceName = configurationProvider.GetSetting(ServiceNameSettingName, isMandatory: false);
            if (!string.IsNullOrEmpty(serviceName) && !string.IsNullOrEmpty(ring))
            {
                serviceName = $"{serviceName} (ring {ring})";
            }

            telemetry.Context.GlobalProperties[ServiceNameSettingName.Replace("_", string.Empty)] = serviceName;

            var instanceName = configurationProvider.GetSetting(InstanceNameSettingName, isMandatory: false);
            telemetry.Context.GlobalProperties[InstanceNameSettingName.Replace("_", string.Empty)] = instanceName;

            if (telemetry.Context?.Cloud == null)
            {
                return;
            }

            // our service represents the role name while the instance itself is the pod name given we will add more pods as we scale
            // this is required to build an application map of services
            telemetry.Context.Cloud.RoleName = serviceName;
            telemetry.Context.Cloud.RoleInstance = instanceName;
        }
    }
}