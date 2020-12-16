using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Delivery.Azure.Library.Exceptions.Traversers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Azure.Library.Exceptions.Writers
{
    /// <summary>
	///     Extensions for improving the logging format of exception messages
	/// </summary>
	public static class ExceptionWriter
	{
		/// <summary>
		///     Formats the exception with extra details and writes it to the <see cref="Trace">trace log</see>
		/// </summary>
		public static string WriteException(this Exception exception, string? customMessage = null)
		{
			try
			{
				if (exception is AggregateException)
				{
					exception = exception.GetBaseException();
				}

				var stringBuilder = new StringBuilder();
				if (!string.IsNullOrEmpty(customMessage))
				{
					stringBuilder.Append($"{customMessage}\r\n");
				}

				stringBuilder.Append(exception.Message);
				var stackTrace = exception.StackTrace;

				var formattedExceptionMessage = new StringBuilder();

				formattedExceptionMessage.Append($"{stringBuilder}\r\n{stackTrace}");
				formattedExceptionMessage.Append(FormatInnerExceptions(exception));

				var dbUpdateException = exception.GetExceptionOrInner<DbUpdateException>();
				if (dbUpdateException != null)
				{
					var entries = dbUpdateException.Entries;

					var innerException = dbUpdateException.InnerException;
					while (innerException != null)
					{
						if (innerException is SqlException sqlException)
						{
							formattedExceptionMessage.Append($"{string.Join(", ", sqlException.Errors.OfType<SqlError>().Select(sqlError => sqlError.Message))}\r\n{string.Join(", ", entries.Select(entry => entry.Entity))}");
							formattedExceptionMessage.Append(FormatInnerExceptions(sqlException));
							break;
						}

						formattedExceptionMessage.Append(FormatInnerExceptions(innerException));
						innerException = innerException.InnerException;
					}
				}

				return formattedExceptionMessage.ToString();
			}
			catch (Exception outerException)
			{
				Trace.TraceError("Error in exception handler: {0}\r\n{1}", outerException.Message, outerException.StackTrace);
				throw;
			}
		}

		private static string FormatInnerExceptions(Exception exception)
		{
			var innerExceptionGeneral = exception.InnerException;
			var innerExceptionMessage = new StringBuilder();
			var innerExceptionCount = 0;

			while (innerExceptionGeneral != null)
			{
				innerExceptionMessage.Append($"{innerExceptionGeneral.Message}:\r\n{innerExceptionGeneral.StackTrace}");
				innerExceptionGeneral = innerExceptionGeneral.InnerException;
				innerExceptionCount++;
				if (innerExceptionCount >= 5)
				{
					break;
				}
			}

			return innerExceptionMessage.ToString();
		}
	}
}