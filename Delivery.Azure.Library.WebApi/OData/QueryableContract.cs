using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.WebApi.OData
{
    [DataContract]
	public sealed class QueryableContract : IEquatable<QueryableContract>
	{
		/// <summary>
		///     Integer value indicating number of records to skip from the result
		/// </summary>
		[Display(Name = "$skip", Description = "Skip 10 Records=10")]
		[DataMember]
		public uint? Skip { get; set; }

		/// <summary>
		///     Integer value indicating number of records to fetch from the result
		/// </summary>
		[Display(Name = "$top", Description = "Get Top 100 Records=100")]
		[DataMember]
		public uint? Top { get; set; } = 100;

		/// <summary>
		///     Enables complex filter criteria using expression trees
		/// </summary>
		[Display(Name = "$filter", Description = "Arithmetic Operators=Value le 333.333" +
		                                         "|Logical Operators=Underwriter eq 'Jane Smith' and PolicyType eq 'IndustryMachinery' or InceptionDate gt datetime'2020-04-01 09:15:53.4240000'" +
		                                         "|String Functions=substringof('ragibull.com',InsertedBy)" +
		                                         "|Date Functions=day(ExpirationDate) eq 21" +
		                                         "|Boolean Operators=not (Underwriter eq 'Jane Smith' and (PolicyType eq 'IndustryMachinery' or InceptionDate gt datetime'2020-04-01 09:15:53.4240000'))")]
		[DataMember]
		public string? Filter { get; set; }

		[Display(Name = "$orderby", Description = "OrderBy Single Field with Expilcit descending sort=InceptionDate desc" +
		                                          "|OrderBy Multiple Fields with explicit ascending sort=Status,PolicyType,InceptionDate,ExpirationDate asc" +
		                                          "|OrderBy Multiple Fields with implicit ascending sort=InceptionDate,ExpirationDate")]
		[DataMember]
		public string? Sort { get; set; }

		private CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;

		// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
		public CultureInfo GetCultureInfo()
		{
			return cultureInfo;
		}

		/// <summary>
		///     Updates the desired culture in a safe way that does not expose the type to open api
		/// </summary>
		/// <param name="culture"></param>
		public void SetCultureInfo(CultureInfo culture) => cultureInfo = culture;

		public bool Equals(QueryableContract? other)
		{
			return Skip == other?.Skip && Top == other?.Top && Filter == other?.Filter && Sort == other?.Sort;
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(objA: null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj.GetType() == GetType() && Equals((QueryableContract) obj);
		}

		public override int GetHashCode()
		{
			var hash = new HashCode();

			if (Skip != null)
			{
				hash.Add(Skip);
			}

			if (Top != null)
			{
				hash.Add(Top);
			}

			if (!string.IsNullOrWhiteSpace(Filter))
			{
				hash.Add(Filter);
			}

			if (Sort != null)
			{
				hash.Add(Sort);
			}

			return hash.ToHashCode();
		}

		public override string ToString()
		{
			return $"{GetType().Name}: " +
			       $"{nameof(Skip)}: {Skip.Format()}, " +
			       $"{nameof(Top)}: {Top.Format()}, " +
			       $"{nameof(Filter)}: {Filter.Format()}, " +
			       $"{nameof(Sort)} : {Sort.Format()}, ";
		}
	}
}