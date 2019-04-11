using System;
using System.Collections.Generic;
using EventRouter.Core;

namespace Logfile.Core
{
	/// <summary>
	/// Represents a logfile configuration.
	/// </summary>
	/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
	public class LogfileConfiguration<TLoglevel>: HubConfiguration<LogEvent<TLoglevel>>
		where TLoglevel : Enum
	{
		/// <summary>
		/// Gets whether the developer mode is enabled (true) which allows log events
		/// with the developer flag set to be forwarded to routers.
		/// </summary>
		public bool IsDeveloperModeEnabled { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LogfileConfiguration"/> class.
		/// </summary>
		/// <param name="routers">The routers. Must not be null, but may be an empty
		///		list.</param>
		/// <param name="preprocessors">The preprocessors. Must not be null, but may be
		///		an empty list.</param>
		/// <param name="maximumLogEventsQueueLength">Maximum queue length for log events.
		///		Must be greater than zero.</param>
		///	<param name="maximumLogEventsForwardingCount">Maximum number of log events
		///		to forward in a single router invocation.</param>
		/// <param name="waitForMoreLogEventsForwardingDelay">The time to wait for
		///		even more log events coming in when forwarding to the routers.</param>
		/// <param name="isDeveloperModeEnabled">Whether to also forward developer
		///		log events.</param>
		///	<exception cref="ArgumentNullException">Thrown if <paramref name="routers"/>,
		///		or <paramref name="preprocessors"/> is null.</exception>
		///	<exception cref="ArgumentOutOfRangeException">Thrown if the property
		///		<c>MaximumRoutablesQueueLength</c> is less than or equal to
		///		zero, the property <c>maximumRoutablesForwardingCount</c> is less than
		///		or equal to zero, or the property <c>waitForMoreRoutablesForwardingDelay"</c>
		///		is less than zero.</exception>
		public LogfileConfiguration(IEnumerable<IRouter<LogEvent<TLoglevel>>> routers,
		IEnumerable<IRoutablePreprocessor<LogEvent<TLoglevel>>> preprocessors,
			int maximumLogEventsQueueLength, int maximumLogEventsForwardingCount,
			TimeSpan waitForMoreLogEventsForwardingDelay, bool isDeveloperModeEnabled)
			: base(routers, preprocessors, maximumLogEventsQueueLength,
				  maximumLogEventsForwardingCount, waitForMoreLogEventsForwardingDelay)
		{
			this.IsDeveloperModeEnabled = isDeveloperModeEnabled;
		}
	}
}
