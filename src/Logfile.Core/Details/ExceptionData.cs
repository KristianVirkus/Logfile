using System;

namespace Logfile.Core.Details
{
	/// <summary>
	/// Implements extension methods for fluent exception data creation.
	/// </summary>
	public static class ExceptionDataExtensions
	{
		/// <summary>
		/// Adds a log event to an <paramref name="exception"/>'s data
		/// dictionary to have it processed automatically when the exception
		/// is logged as detail.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="exception">The exception to extend by a log event.</param>
		/// <param name="loglevel">The loglevel.</param>
		/// <param name="time">The log event time, or null for now.</param>
		/// <returns>The created log event to allow for immediate fluent
		///		enrichment by log event details.</returns>
		///	<exception cref="ArgumentNullException">Thrown if
		///		<paramref name="exception"/> is null.</exception>
		public static LogEvent<TLoglevel> AddLogEvent<TLoglevel>(this Exception exception,
			TLoglevel loglevel, DateTime? time = null)
			where TLoglevel : Enum
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			var logEvent = new LogEvent<TLoglevel>(_ => { }, loglevel, time);
			exception.Data[typeof(LogEvent<TLoglevel>)] = logEvent;
			return logEvent;
		}
	}
}
