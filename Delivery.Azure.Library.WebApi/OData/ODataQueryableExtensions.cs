using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Microsoft.Rest;
using StringToExpression.LanguageDefinitions;


namespace Delivery.Azure.Library.WebApi.OData
{
    public static class ODataQueryableExtensions
    {
        public static IQueryable<T> WithFilterSort<T>(this IQueryable<T> parentQuery, QueryableContract queryable) where T : class
		{
			if (!string.IsNullOrWhiteSpace(queryable.Filter))
			{
				parentQuery = parentQuery.ApplyFilter(queryable.Filter, queryable);
			}

			if (!string.IsNullOrWhiteSpace(queryable.Sort))
			{
				parentQuery = parentQuery.ApplyOrder(queryable.Sort);
			}

			return parentQuery;
		}

		public static IQueryable<T> WithSkipTake<T>(this IQueryable<T> parentQuery, QueryableContract queryable) where T : class
		{
			parentQuery = parentQuery.ApplySkip(queryable.Skip, queryable.Top);
			return parentQuery;
		}

		private static IQueryable<T> ApplyOrder<T>(this IQueryable<T> query, string sortPredicate)
		{
			if (string.IsNullOrWhiteSpace(sortPredicate))
			{
				return query;
			}

			try
			{
				var orderByProperties = sortPredicate.Split(",");
				var sortPredicates = sortPredicate.Split(" ");
				var explicitDescending = sortPredicates.Last().Equals("desc", StringComparison.InvariantCultureIgnoreCase);

				var firstIteration = true;
				orderByProperties.ForEach(orderByProperty =>
				{
					orderByProperty = orderByProperty.Trim().Split(" ").First();
					var type = typeof(T);
					var property = type.GetProperty(orderByProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
					var parameter = Expression.Parameter(type, "p");
					var propertyAccess = Expression.MakeMemberAccess(parameter, property ?? throw new InvalidOperationException($"Property cannot be null ({orderByProperty})"));
					var orderByExpression = Expression.Lambda(propertyAccess, parameter);

					string sortCommand;
					if (firstIteration)
					{
						sortCommand = explicitDescending ? "OrderByDescending" : "OrderBy";
						firstIteration = false;
					}
					else
					{
						sortCommand = explicitDescending ? "ThenByDescending" : "ThenBy";
					}

					var resultExpression = Expression.Call(typeof(Queryable), sortCommand, new[] {type, property!.PropertyType},
						query.Expression, Expression.Quote(orderByExpression));

					query = (IOrderedQueryable<T>) query.Provider.CreateQuery<T>(resultExpression);
				});

				return query;
			}
			catch (Exception exception)
			{
				throw new ValidationException($"Provided sort expression '{sortPredicate}' has incorrect format", exception);
			}
		}

		private static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, string filter, QueryableContract queryable)
		{
			if (string.IsNullOrEmpty(filter))
			{
				return query;
			}

			try
			{
				filter = PrepareInputsForQuerying(filter, queryable);

				var compiledFilter = new ODataFilterLanguage().Parse<T>(filter);

				return query.Where(compiledFilter);
			}
			catch (Exception exception)
			{
				throw new ValidationException($"Provided filter expression '{filter}' has incorrect format", exception);
			}
		}

		private static string PrepareInputsForQuerying(string filter, QueryableContract queryable)
		{
			const string matchPattern = @"'([^']*)'";

			filter = new Regex(matchPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(value: 2)).Replace(filter, match =>
			{
				var matchValue = match.Value.Replace("'", "");

				if (!DateTimeOffset.TryParse(matchValue, queryable.GetCultureInfo(), DateTimeStyles.None, out var parsedDate))
				{
					return match.Value;
				}

				//This is the standard ISO-8601 format
				var parsedDateString = $"datetimeoffset'{parsedDate:O}'";
				return parsedDateString;
			});

			return filter;
		}

		private static IQueryable<T> ApplySkip<T>(this IQueryable<T> query, uint? skip, uint? take)
			=> query
				.SkipIf(skip.HasValue, (int) skip.GetValueOrDefault())
				.TakeIf(take.HasValue, (int) take.GetValueOrDefault());

		private static IQueryable<T> SkipIf<T>(this IQueryable<T> query, bool predicate, int skip)
			=> predicate ? query.Skip(skip) : query;

		private static IQueryable<T> TakeIf<T>(this IQueryable<T> query, bool predicate, int skip)
			=> predicate ? query.Take(skip) : query;
    }
}