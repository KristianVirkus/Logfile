using EventRouter.Core;
using Logfile.Core.Details;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logfile.Core.Preprocessors
{
	/// <summary>
	/// Implements an event preprocessor to extract log events previously added
	/// to logged exception objects' data property. This will be performed at
	/// the time of routing an event.
	/// </summary>
	/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
	class ExtractLogEventsFromExceptions<TLoglevel> : IRoutablePreprocessor<LogEvent<TLoglevel>>
		where TLoglevel : Enum
	{
		/// <summary>
		/// Gets a default instance.
		/// </summary>
		public static ExtractLogEventsFromExceptions<TLoglevel> Instance { get; } = new ExtractLogEventsFromExceptions<TLoglevel>();

		#region IRoutablePreprocessor implementation

		public bool OnEnqueueing => false;

		public IEnumerable<LogEvent<TLoglevel>> Process(LogEvent<TLoglevel> routable)
		{
			if (routable == null) throw new ArgumentNullException(nameof(routable));

			// Pull all additional log events from exception data.
			var additionalLogEvents = this.getAllContainedLogEvents(routable);
			if (additionalLogEvents == null) return null;
			return new LogEvent<TLoglevel>[] { routable }.Concat(additionalLogEvents);
		}

		/// <summary>
		/// Extracts the log events contained in an event's exception objects' data property.
		/// Recursively analyzes all nested log events for more exception objects.
		/// </summary>
		/// <param name="logEvent">The log event to examine.</param>
		/// <returns>The contained log events.</returns>
		IEnumerable<LogEvent<TLoglevel>> getAllContainedLogEvents(LogEvent<TLoglevel> logEvent)
		{
			var containedLogEvents = new List<LogEvent<TLoglevel>>();
			var additionalLogEventLists = logEvent.Details
										.OfType<ExceptionDetail>()
										.Where(d => (d.ExceptionObject?.Data?.Contains(typeof(LogEvent<TLoglevel>)) == true)
														&& (d.ExceptionObject?.Data[typeof(LogEvent<TLoglevel>)] is List<LogEvent<TLoglevel>>))
										.Select(d => (List<LogEvent<TLoglevel>>)d.ExceptionObject.Data[typeof(LogEvent<TLoglevel>)]).ToList();
			foreach (var additionalLogEventList in additionalLogEventLists)
			{
				foreach (var additionalLogEvent in additionalLogEventList)
				{
					containedLogEvents.Add(additionalLogEvent);
					var evenMoreLogEvents = this.getAllContainedLogEvents(additionalLogEvent);
					if (evenMoreLogEvents?.Any() == true) containedLogEvents.AddRange(evenMoreLogEvents);
				}
			}

			return containedLogEvents;
		}

		#endregion
	}
}
