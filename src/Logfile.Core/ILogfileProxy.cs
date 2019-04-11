using System;
using System.Collections.Generic;

namespace Logfile.Core
{
	/// <summary>
	/// Common interface of all logfile implementations, grouping
	/// features and the like behaving just like a logfile.
	/// </summary>
	/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
	public interface ILogfileProxy<TLoglevel>
		where TLoglevel: Enum
	{
		/// <summary>
		/// Gets the logfile proxy hierarchy. This gets extended when cloning a logfile
		/// via the <c>Clone</c> method.
		/// </summary>
		IEnumerable<string> Hierarchy { get; }

		/// <summary>
		/// Creates a new log event.
		/// </summary>
		/// <param name="loglevel">The loglevel to initialise the event with.</param>
		/// <returns>The log event.</returns>
		LogEvent<TLoglevel> New(TLoglevel loglevel);

		/// <summary>
		/// Clones the logfile (proxy) and gives the clone
		/// a new name.
		/// </summary>
		/// <param name="name">The clone name, null to create an unnamed clone.</param>
		/// <returns>The logfile (proxy) clone.</returns>
		ILogfileProxy<TLoglevel> Clone(string name = default);

		/// <summary>
		/// Logs <paramref name="events"/>. Ignores if <paramref name="events"/>
		/// is null in order to not interrupt any application logic.
		/// </summary>
		/// <param name="events">The events.</param>
		void Log(IEnumerable<LogEvent<TLoglevel>> events);
	}
}
