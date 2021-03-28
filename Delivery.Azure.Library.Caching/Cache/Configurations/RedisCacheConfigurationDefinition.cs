using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Azure.Library.Caching.Cache.Configurations
{
    public class RedisCacheConfigurationDefinition : SecureConfigurationDefinition
    {
        /// <summary>
        ///     The full connection string to the redis cache instance
        /// </summary>
        public virtual async Task<string> GetConnectionStringAsync()
        {
            return await SecretProvider.GetSecretAsync("RedisCache-ConnectionString");
        }
        
        public virtual string GetConnectionString()
        {
            var connectionString = ConfigurationProvider.GetSetting("RedisCache-ConnectionString");

            // ServiceStack requires a certain redis connection string format
            // https://github.com/ServiceStack/ServiceStack.Redis#redis-connection-strings
            var host = connectionString.Split(separator: ':')[0];
            var port = int.Parse(connectionString.Split(separator: ':')[1].Split(separator: ',')[0]);
            string formattedConnectionString = $"{host}:{port}";

            var passwordSection = "password=";
            if (connectionString.Contains(passwordSection))
            {
                var password = connectionString.Split(passwordSection)[1].Split(separator: ',')[0];
                formattedConnectionString = $"{password}@{formattedConnectionString}";
            }

            var sslSection = "ssl=";
            if (connectionString.Contains(sslSection))
            {
                var ssl = connectionString.Split(sslSection)[1].Split(separator: ',')[0];
                formattedConnectionString = $"{formattedConnectionString}?ssl={ssl}";
            }

            return formattedConnectionString;
        }

        /// <summary>
        ///     How long an item should stay in the cache
        /// </summary>
        public virtual int DefaultCacheExpirySeconds => ConfigurationProvider.GetSettingOrDefault<int>("RedisCache_DefaultExpirySeconds", 4 * 60 * 60);

        public RedisCacheConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}