using Logfile.Core.Details;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logfile.Core
{
	/// <summary>
	/// Implements a simple logfile proxy to be used as logfile clones.
	/// </summary>
	/// <typeparam name="TLoglevel">The logfile type.</typeparam>
	class LogfileProxy<TLoglevel> : ILogfileProxy<TLoglevel>
		where TLoglevel : Enum
	{
		#region Fields

		readonly Action<IEnumerable<LogEvent<TLoglevel>>> logCallback;

		#endregion

		#region Constructors

		public LogfileProxy(IEnumerable<string> hierarchy, string name, Action<IEnumerable<LogEvent<TLoglevel>>> logCallback)
		{
			this.Hierarchy = hierarchy;
			if (name != null)
				this.Hierarchy = hierarchy.Concat(new string[] { name });

			this.logCallback = logCallback;
		}

		#endregion

		#region ILogfileProxy<TLoglevel> implementation

		public IEnumerable<string> Hierarchy { get; }

		public LogEvent<TLoglevel> New(TLoglevel loglevel)
		{
			return new LogEvent<TLoglevel>((_logEvent) => this.Log(new[] { _logEvent }), loglevel);
		}

		public ILogfileProxy<TLoglevel> Clone(string name = null)
		{
			return new LogfileProxy<TLoglevel>(this.Hierarchy, name, this.Log);
		}

		public void Log(IEnumerable<LogEvent<TLoglevel>> events)
		{
			try
			{
				// Add hierarchy where not yet present.
				foreach (var logEvent in events.Where(e => !e.Details.OfType<LogfileHierarchy>().Any()))
				{
					logEvent.Details.Insert(0, new LogfileHierarchy(this.Hierarchy));
				}

				this.logCallback?.Invoke(events);
			}
			catch
			{
				// TODO Log.
			}
		}

		#endregion
	}
}
