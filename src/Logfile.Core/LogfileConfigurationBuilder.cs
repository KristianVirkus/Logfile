using EventRouter.Core;
using Logfile.Core.Preprocessors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logfile.Core
{
	/// <summary>
	/// Implements a builder for the <see cref="LogfileConfiguration{TLoglevel}"/>
	/// class.
	/// </summary>
	/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
	public class LogfileConfigurationBuilder<TLoglevel>
		where TLoglevel : Enum
	{
		const int MaximumRoutablesQueueLengthDefault = 100;
		const int MaximumRoutablesForwardingCountDefault = 100;
		static readonly TimeSpan WaitForMoreRoutablesForwardingDelayDefault = TimeSpan.FromMilliseconds(100);

		/// <summary>
		/// Gets the routers.
		/// </summary>
		public IList<IRouter<LogEvent<TLoglevel>>> Routers { get; }

		/// <summary>
		/// Gets the preprocessors.
		/// </summary>
		public IList<IRoutablePreprocessor<LogEvent<TLoglevel>>> Preprocessors { get; }

		/// <summary>
		/// Gets the maximum length of the queue for routables.
		/// </summary>
		public int MaximumRoutableQueueLength { get; set; }

		/// <summary>
		/// Gets the maximum number of routables to forward at once.
		/// </summary>
		public int MaximumRoutablesForwardingCount { get; set; }

		/// <summary>
		/// Gets the time to wait for more routables to be forwarded
		/// before actually starting to forward any routables.
		/// </summary>
		public TimeSpan WaitForMoreRoutablesForwardingDelay { get; set; }

		/// <summary>
		/// Gets the allowed loglevels. If any loglevels set, no other loglevls
		/// will be accepted.
		/// </summary>
		public IList<TLoglevel> AllowLoglevels { get; }

		/// <summary>
		/// Gets the blocked loglevels. Blocked loglevels are checked after potentially
		/// set allowed loglevels have been checked.
		/// </summary>
		public IList<TLoglevel> BlockLoglevels { get; }

		/// <summary>
		/// Gets or set whether the developer mode is enabled.
		/// </summary>
		public bool IsDeveloperModeEnabled { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LogfileConfigurationBuilder{TLoglevel}"/>
		/// class.
		/// </summary>
		public LogfileConfigurationBuilder()
		{
			this.Routers = new List<IRouter<LogEvent<TLoglevel>>>();
			this.Preprocessors = new List<IRoutablePreprocessor<LogEvent<TLoglevel>>>();
			this.MaximumRoutableQueueLength = MaximumRoutablesQueueLengthDefault;
			this.MaximumRoutablesForwardingCount = MaximumRoutablesForwardingCountDefault;
			this.WaitForMoreRoutablesForwardingDelay = WaitForMoreRoutablesForwardingDelayDefault;
			this.AllowLoglevels = new List<TLoglevel>();
			this.BlockLoglevels = new List<TLoglevel>();
		}

		/// <summary>
		/// Builds a new instance of the <see cref="LogfileConfiguration{TLoglevel}"/>
		/// based on the settings within this builder instance.
		/// </summary>
		/// <returns>The logfile configuration.</returns>
		///	<exception cref="ArgumentOutOfRangeException">Thrown if the property
		///		<c>MaximumRoutablesQueueLength</c> is less than or equal to
		///		zero, the property <c>maximumRoutablesForwardingCount</c> is less than
		///		or equal to zero, or the property <c>waitForMoreRoutablesForwardingDelay"</c>
		///		is less than zero.</exception>
		public LogfileConfiguration<TLoglevel> Build()
		{
			var loglevelFilter = new LoglevelFilter<TLoglevel>(this.AllowLoglevels, this.BlockLoglevels);
			return new LogfileConfiguration<TLoglevel>(
				this.Routers,
				this.Preprocessors.Concat(new IRoutablePreprocessor<LogEvent<TLoglevel>>[] { loglevelFilter }),
				this.MaximumRoutableQueueLength,
				this.MaximumRoutablesForwardingCount,
				this.WaitForMoreRoutablesForwardingDelay,
				this.IsDeveloperModeEnabled);
		}
	}

	/// <summary>
	/// Implements extension methods for fluent logfile configuration.
	/// </summary>
	public static class LogfileConfigurationBuilderExtensions
	{
		/// <summary>
		/// Adds a router.
		/// </summary>
		/// <typeparam name="T">The loglevel type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="router">The router to add.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		/// <exception cref="ArgumentNullException">Thrown if either
		///		<paramref name="self"/> or <paramref name="router"/> is null.</exception>
		public static LogfileConfigurationBuilder<TLoglevel> AddRouter<TLoglevel>(this LogfileConfigurationBuilder<TLoglevel> self, IRouter<LogEvent<TLoglevel>> router)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (router == null) throw new ArgumentNullException(nameof(router));
			self.Routers.Add(router);
			return self;
		}

		/// <summary>
		/// Adds a preprocessor.
		/// </summary>
		/// <typeparam name="T">The loglevel type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="preprocessor">The preprocessor to add.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		/// <exception cref="ArgumentNullException">Thrown if either
		///		<paramref name="self"/> or <paramref name="preprocessor"/> is null.</exception>
		public static LogfileConfigurationBuilder<TLoglevel> AddPreprocessor<TLoglevel>(this LogfileConfigurationBuilder<TLoglevel> self, IRoutablePreprocessor<LogEvent<TLoglevel>> preprocessor)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (preprocessor == null) throw new ArgumentNullException(nameof(preprocessor));
			self.Preprocessors.Add(preprocessor);
			return self;
		}

		/// <summary>
		/// Sets the maximum length of the queue for routables.
		/// </summary>
		/// <typeparam name="TLoglevel">The event type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="value">The maximum queue length.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="self"/> is null.</exception>
		public static LogfileConfigurationBuilder<TLoglevel> SetMaximumRoutableQueueLength<TLoglevel>(this LogfileConfigurationBuilder<TLoglevel> self, int value)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			self.MaximumRoutableQueueLength = value;
			return self;
		}

		/// <summary>
		/// Set the maximum number of routables to forward at once.
		/// </summary>
		/// <typeparam name="TLoglevel">The event type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="value">The count.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="self"/> is null.</exception>
		public static LogfileConfigurationBuilder<TLoglevel> SetMaximumRoutablesForwardingCount<TLoglevel>(this LogfileConfigurationBuilder<TLoglevel> self, int value)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			self.MaximumRoutablesForwardingCount = value;
			return self;
		}

		/// <summary>
		/// Sets the time to wait for more routables to be forwarded
		/// before actually starting to forward any routables.
		/// </summary>
		/// <typeparam name="TLoglevel">The event type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="value">The delay.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static LogfileConfigurationBuilder<TLoglevel> SetWaitForMoreRoutablesForwardingDelay<TLoglevel>(this LogfileConfigurationBuilder<TLoglevel> self, TimeSpan value)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			self.WaitForMoreRoutablesForwardingDelay = value;
			return self;
		}

		/// <summary>
		/// Allows log events with the specified <paramref name="loglevel"/> to be forwarded.
		/// </summary>
		/// <remarks>If there have any loglevels been allowed an event's loglevel must be
		///		amongst the allowed loglevels in order to get forwarded.</remarks>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="loglevel">The loglevel to allow.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static LogfileConfigurationBuilder<TLoglevel> AllowLoglevel<TLoglevel>(
			this LogfileConfigurationBuilder<TLoglevel> self, TLoglevel loglevel)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (!self.AllowLoglevels.Contains(loglevel))
				self.AllowLoglevels.Add(loglevel);
			return self;
		}

		/// <summary>
		/// Allows log events with the loglevels specified within the range of
		/// <paramref name="from"/> and <paramref name="to"/>.
		/// </summary>
		/// <remarks>The order of <paramref name="from"/> and <paramref name="to"/>
		///		is irrelevant. If there have any loglevels been allowed an event's loglevel
		///		must be amongst the allowed loglevels in order to get forwarded.</remarks>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="from">The more detailed loglevel to allow.</param>
		/// <param name="to">The more critical loglevel to allow.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static LogfileConfigurationBuilder<TLoglevel> AllowLoglevels<TLoglevel>(
			this LogfileConfigurationBuilder<TLoglevel> self,
			TLoglevel from, TLoglevel to)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (from.CompareTo(to) > 0)
			{
				var temp = to;
				to = from;
				from = temp;
			}

			var from_ = from;
			var loglevels = from l in Enum.GetValues(typeof(TLoglevel)).Cast<TLoglevel>()
							where l.CompareTo(from_) >= 0 && l.CompareTo(to) <= 0
							select l;

			foreach (var loglevel in loglevels.Except(self.AllowLoglevels).ToList())
			{
				self.AllowLoglevels.Add(loglevel);
			}

			return self;
		}

		/// <summary>
		/// Blocks log events with the specified <paramref name="loglevel"/> from being forwarded.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="loglevel">The loglevel to block.</param>
		/// <returns>The same configuration builder instance to allow fluent syntax.</returns>
		public static LogfileConfigurationBuilder<TLoglevel> BlockLoglevel<TLoglevel>(
			this LogfileConfigurationBuilder<TLoglevel> self, TLoglevel loglevel)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (!self.BlockLoglevels.Contains(loglevel))
				self.BlockLoglevels.Add(loglevel);
			return self;
		}

		/// <summary>
		/// Blocks log events with the loglevels specified within the range of
		/// <paramref name="from"/> and <paramref name="to"/>.
		/// </summary>
		/// <remarks>The order of <paramref name="from"/> and <paramref name="to"/>
		///		is irrelevant.</remarks>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="from">The more detailed loglevel to block.</param>
		/// <param name="to">The more critical loglevel to block.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static LogfileConfigurationBuilder<TLoglevel> BlockLoglevels<TLoglevel>(
			this LogfileConfigurationBuilder<TLoglevel> self,
			TLoglevel from, TLoglevel to)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (from.CompareTo(to) > 0)
			{
				var temp = to;
				to = from;
				from = temp;
			}

			var from_ = from;
			var loglevels = from l in Enum.GetValues(typeof(TLoglevel)).Cast<TLoglevel>()
							where l.CompareTo(from_) >= 0 && l.CompareTo(to) <= 0
							select l;

			foreach (var loglevel in loglevels.Except(self.BlockLoglevels).ToList())
			{
				self.BlockLoglevels.Add(loglevel);
			}

			return self;
		}

		/// <summary>
		/// Enables the developer mode.
		/// </summary>
		/// <remarks>Enabling the developer mode allows log events to be forwarded which
		///		had been flagged as developer events. Thus, a developer may still use
		///		the loglevel to distinguish different types of events while completely
		///		turning on or off certain events not intended for public audience.</remarks>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static LogfileConfigurationBuilder<TLoglevel> EnableDeveloperMode<TLoglevel>(
			this LogfileConfigurationBuilder<TLoglevel> self)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			self.IsDeveloperModeEnabled = true;
			return self;
		}

		/// <summary>
		/// Enables the developer mode when compiled in with DEBUG compiler flag.
		/// </summary>
		/// <remarks>Enabling the developer mode allows log events to be forwarded which
		///		had been flagged as developer events. Thus, a developer may still use
		///		the loglevel to distinguish different types of events while completely
		///		turning on or off certain events not intended for public audience.</remarks>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static LogfileConfigurationBuilder<TLoglevel> EnableDeveloperModeForDebugCompilerFlag<TLoglevel>(
			this LogfileConfigurationBuilder<TLoglevel> self)
			where TLoglevel : Enum
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
#if DEBUG
			self.IsDeveloperModeEnabled = true;
#endif
			return self;
		}

		/// <summary>
		/// Enables the automatic extraction of log events stored in exception data.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="configurationBuilder">The configuration builder.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static LogfileConfigurationBuilder<TLoglevel> UseLogEventsFromExceptionData<TLoglevel>(
			this LogfileConfigurationBuilder<TLoglevel> configurationBuilder)
			where TLoglevel : Enum
		{
			configurationBuilder.AddPreprocessor(ExtractLogEventsFromExceptions<TLoglevel>.Instance);
			return configurationBuilder;
		}
	}
}
