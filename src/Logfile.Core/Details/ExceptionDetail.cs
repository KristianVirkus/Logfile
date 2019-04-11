using System;

namespace Logfile.Core.Details
{
	/// <summary>
	/// Represents exception log event details.
	/// </summary>
	public class ExceptionDetail
	{
		/// <summary>
		/// Gets the exception object.
		/// </summary>
		public Exception ExceptionObject { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionDetail"/> class.
		/// </summary>
		/// <param name="exception">The exception object.</param>
		public ExceptionDetail(Exception exception)
		{
			this.ExceptionObject = exception;
		}

		public override string ToString() => this.ExceptionObject?.ToString() ?? "null";
	}

	/// <summary>
	/// Implements extension methods for fluent event creation.
	/// </summary>
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Adds an <paramref name="exception"/> to a <paramref name="logEvent"/>.
		/// </summary>
		/// <typeparam name="TLoglevel">The log event type.</typeparam>
		/// <param name="logEvent">The log event.</param>
		/// <param name="exception">The exception. If null, no exception will be added.</param>
		/// <returns><paramref name="logEvent"/></returns>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logEvent"/> is null.</exception>
		public static LogEvent<TLoglevel> Exception<TLoglevel>(this LogEvent<TLoglevel> logEvent, Exception exception)
			where TLoglevel : Enum
		{
			if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

			if (exception != null) logEvent.Details.Add(new ExceptionDetail(exception));
			return logEvent;
		}
	}
}
