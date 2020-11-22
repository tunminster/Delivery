using System;
using System.Collections.Generic;
using System.Linq;

namespace Delivery.Azure.Library.Exceptions.Traversers
{
	public static class ExceptionStackTraceTraverser
	{
		/// <summary>
		///     Tries to find an exception of a given type inside the inner exception loop
		///     If the exception is an <see cref="AggregateException" /> then the list is exploded and the first
		///     <typeparamref name="TException" /> is returned
		/// </summary>
		/// <remarks>
		///     A limit of 10 nested exceptions is used to prevent infinite loops e.g. with entity framework exceptions which
		///     reference each other as inner exceptions
		/// </remarks>
		public static TException GetExceptionOrInner<TException>(this Exception exception)
			where TException : Exception
		{
			var exceptions = new List<Exception> {exception};

			if (exception is AggregateException aggregateException)
			{
				exceptions = aggregateException.InnerExceptions.ToList();
			}

			foreach (var innerException in exceptions)
			{
				var innerExceptionCount = 0;
				var exceptionCandidate = innerException;

				while (exceptionCandidate != null)
				{
					if (exceptionCandidate is TException exceptionResolved)
					{
						return exceptionResolved;
					}

					if (exceptionCandidate.InnerException != null)
					{
						exceptionCandidate = exceptionCandidate.InnerException;
					}

					innerExceptionCount++;
					if (innerExceptionCount >= 10)
					{
						break;
					}
				}
			}

			return null;
		}
	}
}