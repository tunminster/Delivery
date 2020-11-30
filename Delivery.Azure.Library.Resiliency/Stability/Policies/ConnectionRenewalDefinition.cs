using System;
using System.Threading.Tasks;

namespace Delivery.Azure.Library.Resiliency.Stability.Policies
{
    internal class ConnectionRenewalDefinition
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="healthPredicate">Function to run when the connection needs to be renewed</param>
        /// <param name="connectionRenewal">Predicate determining whether or not the connection is still considered healthy</param>
        public ConnectionRenewalDefinition(Func<Exception, bool> healthPredicate, Func<Task> connectionRenewal)
        {
            ConnectionRenewal = connectionRenewal;
            HealthPredicate = healthPredicate;
        }

        /// <summary>
        ///     Function to run when the connection needs to be renewed
        /// </summary>
        public Func<Task> ConnectionRenewal { get; }

        /// <summary>
        ///     Predicate determining whether or not the connection is still considered healthy
        /// </summary>
        public Func<Exception, bool> HealthPredicate { get; }
    }
}