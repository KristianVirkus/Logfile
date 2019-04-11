using EventRouter.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logfile.Core.Preprocessors
{
	/// <summary>
	/// Implements a loglevel filter as routable preprocessor.
	/// </summary>
	/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
	public class LoglevelFilter<TLoglevel> : IRoutablePreprocessor<LogEvent<TLoglevel>>
		where TLoglevel : Enum
	{
		static readonly IEnumerable<LogEvent<TLoglevel>> NoLogEvents;

		/// <summary>
		/// Gets the allowed loglevels a log event to be forwarded must be of,
		/// if any loglevels are defined.
		/// </summary>
		public IEnumerable<TLoglevel> AllowLoglevels { get; }

		/// <summary>
		/// Gets the blocked loglevels a log event to be forwarded must not be of.
		/// </summary>
		public IEnumerable<TLoglevel> BlockLoglevels { get; }

		static LoglevelFilter()
		{
			NoLogEvents = new LogEvent<TLoglevel>[0];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoglevelFilter{TLoglevel}"/>.
		/// </summary>
		/// <param name="allowLoglevels">The allowed loglevels. If any defined,
		///		the log events' loglevel must be within these.</param>
		/// <param name="blockLoglevels">The blocked loglevels. The log events'
		///		loglevel must not be within these.</param>
		public LoglevelFilter(IEnumerable<TLoglevel> allowLoglevels,
			IEnumerable<TLoglevel> blockLoglevels)
		{
			if (allowLoglevels == null) throw new ArgumentNullException(nameof(allowLoglevels));
			if (blockLoglevels == null) throw new ArgumentNullException(nameof(blockLoglevels));

			this.AllowLoglevels = allowLoglevels.ToList();
			this.BlockLoglevels = blockLoglevels.ToList();
		}

		public bool OnEnqueueing => true;

		public IEnumerable<LogEvent<TLoglevel>> Process(LogEvent<TLoglevel> routable)
		{
			if (routable == null) throw new ArgumentNullException(nameof(routable));

			// If routable is forced, do not apply any loglevel filters.
			if (routable.IsForced) return null;

			// If the routable's loglevel shall be blocked, return empty list of
			// replacement routables.
			if (this.BlockLoglevels.Contains(routable.Loglevel)) return NoLogEvents;

			// If there are any allowed loglevels defined, this routable's loglevel
			// must be within the allowed loglevels.
			if (this.AllowLoglevels.Any())
			{
				if (!this.AllowLoglevels.Contains(routable.Loglevel)) return NoLogEvents;
			}

			// Return null to treat as "untouched."
			return null;
		}
	}
}
