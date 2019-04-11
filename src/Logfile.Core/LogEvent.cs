using EventRouter.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Logfile.Core
{
	/// <summary>
	/// Represents a single log event.
	/// </summary>
	/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
	public class LogEvent<TLoglevel> : IRoutable
		where TLoglevel : Enum
	{
		public delegate void LogCallbackDelegate(LogEvent<TLoglevel> logEvent);

		readonly LogCallbackDelegate logCallback;

		/// <summary>
		/// Gets the log event's occurrence time.
		/// </summary>
		public DateTime Time { get; }

		/// <summary>
		/// Gets the log event's loglevel.
		/// </summary>
		public TLoglevel Loglevel { get; }

		/// <summary>
		/// Gets whether forwarding the event is forced independently from
		/// any configuration.
		/// </summary>
		public bool IsForced { get; private set; }

		/// <summary>
		/// Forces the forwarding of the event independently from any configuration.
		/// For this to be successful, any configured preprocessors must regard this flag.
		/// </summary>
		public LogEvent<TLoglevel> Force
		{
			get
			{
				this.IsForced = true;
				return this;
			}
		}

		/// <summary>
		/// Gets whether the event is a developer event and thus possibly not intended
		/// for public audience.
		/// </summary>
		public bool IsDeveloper { get; private set; }

		/// <summary>
		/// Sets the event to be a developer event possibly not intended for public audience.
		/// </summary>
		public LogEvent<TLoglevel> Developer
		{
			get
			{
				this.IsDeveloper = true;
				return this;
			}
		}

		/// <summary>
		/// Gets the source file path of the log event creator (invocation of the Log method.)
		/// </summary>
		public string CallerFilePath { get; private set; }

		/// <summary>
		/// Gets the source member name of the log event creator (invocation of the Log method.)
		/// </summary>
		public string CallerMemberName { get; private set; }

		/// <summary>
		/// Gets the source file line number of the log event creator (invocation of the Log method.)
		/// </summary>
		public int? CallerLineNumber { get; private set; }

		/// <summary>
		/// Gets the log event data.
		/// </summary>
		public IList<object> Details { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LogEvent"/> class.
		/// </summary>
		/// <param name="logCallback">The callback method to invoke when the
		///		<c>Log</c> method is called.</param>
		/// <param name="loglevel">The log event's loglevel.</param>
		/// <param name="time">The time of the log event.</param>
		internal LogEvent(LogCallbackDelegate logCallback, TLoglevel loglevel, DateTime? time = default)
		{
			this.logCallback = logCallback ?? throw new ArgumentNullException(nameof(logCallback));
			this.Loglevel = loglevel;
			this.Time = time ?? DateTime.Now;
			this.Details = new List<object>();
		}

		/// <summary>
		/// Logs the event.
		/// </summary>
		/// <param name="CallerFilePath">File path of the calling source code file.
		///		Automatically to be put in while invocation.</param>
		/// <param name="CallerMemberName">Name of the member in the calling source code.
		///		Automatically to be put in while invocation.</param>
		/// <param name="CallerLineNumber">Line number of the calling source code.
		///		Automatically to be put in while invocation.</param>
		public void Log([CallerFilePath] string CallerFilePath = default,
			[CallerMemberName] string CallerMemberName = default,
			[CallerLineNumber] int? CallerLineNumber = default)
		{
			this.CallerFilePath = CallerFilePath;
			this.CallerMemberName = CallerMemberName;
			this.CallerLineNumber = CallerLineNumber;

			try
			{
				this.logCallback?.Invoke(this);
			}
			catch
			{
			}
		}
	}
}
