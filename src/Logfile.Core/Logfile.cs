using EventRouter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Logfile.Core
{
    /// <summary>
    /// Implements the default logfile.
    /// </summary>
    /// <typeparam name="TLoglevel">The loglevel type.</typeparam>
    public class Logfile<TLoglevel> : Hub<LogEvent<TLoglevel>>, ILogfileProxy<TLoglevel>
        where TLoglevel : Enum
    {
        readonly SemaphoreSlim sync = new SemaphoreSlim(1);
        LogfileConfiguration<TLoglevel> configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logfile"/> class.
        /// </summary>
        public Logfile()
            : base()
        {
            this.Hierarchy = new string[0];
        }

        /// <summary>
        /// Gets the hierarchy of cloned logfiles which led to this instance.
        /// </summary>
		public IEnumerable<string> Hierarchy { get; }

        /// <summary>
        /// DO NOT CALL THIS DIRECTLY.
        /// Use <c>Logfile&lt;TLoglevel&gt;.ReconfigureAsync</c> instead.
        /// </summary>
        public override Task ReconfigureAsync(HubConfiguration<LogEvent<TLoglevel>> configuration,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Do not call the IHub<T>.ReconfigureAsync method directly. Use Logfile<TLoglevel>.ReconfigureAsync instead.");
        }

        /// <summary>
        /// Reconfigures the logfile. The logfile can be stopped by using
        /// null for <paramref name="configuration"/>. The disposable
        /// preprocessors and routers from the current configuration
        /// will get disposed first.
        /// </summary>
        /// <param name="configuration">The new configuration or null
        ///		to stop the logfile.</param>
        /// <param name="cancellationToken">The cancellation token to
        ///		cancel reconfiguring. The instance's state might be
        ///		undefined if canceled.</param>
        ///	<exception cref="TaskCanceledException">Thrown if
        ///		<paramref name="cancellationToken"/> is canceled.</exception>
        public virtual async Task ReconfigureAsync(LogfileConfiguration<TLoglevel> configuration,
            CancellationToken cancellationToken)
        {
            // Just call base configuration change, set new logfile configuration and
            // tolerate some time of inconsistency. Reconfiguring on the fly is just not
            // a usual use case.
            await this.sync.WaitAsync(cancellationToken);
            try
            {
                if (this.configuration != null)
                {
                    foreach (var disposable in this.configuration.Preprocessors.OfType<IDisposable>())
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch
                        {
                            // TODO Log.
                        }
                    }

                    foreach (var disposable in this.configuration.Routers.OfType<IDisposable>())
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch
                        {
                            // TODO Log.
                        }
                    }
                }

                this.configuration = configuration;
                await base.ReconfigureAsync(configuration, cancellationToken);
            }
            finally
            {
                this.sync.Release();
            }
        }

        /// <summary>
        /// Creates a new log event.
        /// </summary>
        /// <param name="loglevel">The loglevel to initialise the event with.</param>
        /// <returns>The log event.</returns>
        public LogEvent<TLoglevel> New(TLoglevel loglevel)
        {
            return new LogEvent<TLoglevel>((_logEvent) => this.Log(new[] { _logEvent }), loglevel);
        }

        /// <summary>
        /// Clones the logfile (proxy) and gives the clone
        /// a new name.
        /// </summary>
        /// <param name="name">The clone name, null to create an unnamed clone.</param>
        /// <returns>The logfile (proxy) clone.</returns>
        public ILogfileProxy<TLoglevel> Clone(string name = default)
        {
            return new LogfileProxy<TLoglevel>(this.Hierarchy, name, this.Log);
        }

        /// <summary>
        /// Logs <paramref name="events"/>. Ignores if <paramref name="events"/>
        /// is null in order to not interrupt any application logic.
        /// </summary>
        /// <param name="events">The events.</param>
        public void Log(IEnumerable<LogEvent<TLoglevel>> events)
        {
            try
            {
                if (events?.Any() != true) return;

                var configuration = this.configuration;
                if (configuration != null)
                {
                    // Remove developer events if forwarding is not enabled.
                    if (!configuration.IsDeveloperModeEnabled)
                        events = events.Where(e => (!e.IsDeveloper && !e.IsForced) || (e.IsForced));
                }

                this.Forward(events);
            }
            catch
            {
                // TODO Log.
            }
        }
    }
}
