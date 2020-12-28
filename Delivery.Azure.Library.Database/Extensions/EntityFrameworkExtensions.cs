using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Azure.Library.Database.Extensions
{
    public static class EntityFrameworkExtensions
    {
        public static IQueryable<object>? Set(this DbContext dbContext, Type type)
        {
            return (IQueryable<object>?) dbContext.GetType().GetMethod("Set")?.MakeGenericMethod(type).Invoke(dbContext, parameters: null);
        }

        public static IQueryable<T>? GetQuery<T>(this DbContext dbContext, Type entityType)
        {
            var pq = from p in dbContext.GetType().GetProperties()
                where p.PropertyType.IsGenericType
                      && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                      && p.PropertyType.GenericTypeArguments[0] == entityType
                select p;
            var prop = pq.Single();

            return (IQueryable<T>?) prop.GetValue(dbContext);
        }
    }
}