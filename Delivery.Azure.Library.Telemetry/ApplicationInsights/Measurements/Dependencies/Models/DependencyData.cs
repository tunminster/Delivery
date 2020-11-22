namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models
{
    public class DependencyData
    {
        public DependencyData(string action)
        {
            Action = action;
        }

        public DependencyData(string action, object? metadata) : this(action)
        {
            Metadata = metadata;
        }

        public string Action { get; }
        public object? Metadata { get; }
    }
}