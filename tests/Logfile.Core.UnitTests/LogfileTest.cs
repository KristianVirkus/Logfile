using EventRouter.Core;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Logfile.Core.UnitTests
{
	class LogfileTest
	{
		void prepare(out Logfile<StandardLoglevel> logfile, out TestRouter<LogEvent<StandardLoglevel>> router)
		{
			router = new TestRouter<LogEvent<StandardLoglevel>>();
			var logfileConfiguration = (new LogfileConfigurationBuilder<StandardLoglevel>()
					.AddRouter(router)
					as LogfileConfigurationBuilder<StandardLoglevel>)
				.Build();
			logfile = new Logfile<StandardLoglevel>();
			logfile.ReconfigureAsync(logfileConfiguration, default).GetAwaiter().GetResult();
		}

		#region ReconfigureAsync

		[Test]
		public void ReconfigureAsyncOnIHub_ShouldThrow_NotSupportedException()
		{
			this.prepare(out var logfile, out var _);
			Assert.ThrowsAsync<NotSupportedException>(async () => await ((IHub<LogEvent<StandardLoglevel>>)logfile).ReconfigureAsync(null, default));
		}

		[Test]
		public async Task ReconfigureAsyncOnLogfileConfigurationNull_Should_RemoveConfiguration()
		{
			this.prepare(out var logfile, out var _);
			await logfile.ReconfigureAsync(null, default);
			logfile.Configuration.Should().BeNull();
		}

		[Test]
		public async Task ReconfigureAsyncOnLogfile_Should_DisposeOldPreprocessorsAndRoutersAndReplaceConfiguration()
		{
			var preprocessor = new TestPreprocessor();
			var router = new TestRouter<LogEvent<StandardLoglevel>>();
			var logfileConfiguration = (new LogfileConfigurationBuilder<StandardLoglevel>()
					.AddPreprocessor(preprocessor)
					.AddRouter(router)
					as LogfileConfigurationBuilder<StandardLoglevel>)
				.Build();
			var logfile = new Logfile<StandardLoglevel>();
			await logfile.ReconfigureAsync(logfileConfiguration, default);
			var preprocessorDisposed = false;
			((TestPreprocessor)logfile.Configuration.Preprocessors.OfType<TestPreprocessor>().Single()).DisposeCallback =
				() => preprocessorDisposed = true;
			var routerDisposed = false;
			((TestRouter<LogEvent<StandardLoglevel>>)logfile.Configuration.Routers.Single()).DisposeCallback =
				() => routerDisposed = true;
			var newRouter = new TestRouter<LogEvent<StandardLoglevel>>();
			var newLogfileConfiguration = (new LogfileConfigurationBuilder<StandardLoglevel>()
					.AddRouter(newRouter)
					as LogfileConfigurationBuilder<StandardLoglevel>)
				.Build();
			await logfile.ReconfigureAsync(newLogfileConfiguration, default);
			preprocessorDisposed.Should().BeTrue();
			routerDisposed.Should().BeTrue();
			logfile.Configuration.Should().BeSameAs(newLogfileConfiguration);
		}

		[Test]
		public async Task ReconfigureAsyncOnLogfileWithExceptionInPreprocessorDispose_Should_DisposeOtherPreprocessorsAnyway()
		{
			var preprocessor = new TestPreprocessor();
			var router = new TestRouter<LogEvent<StandardLoglevel>>();
			var logfileConfiguration = (new LogfileConfigurationBuilder<StandardLoglevel>()
					.AddPreprocessor(preprocessor)
					.AddRouter(router)
					as LogfileConfigurationBuilder<StandardLoglevel>)
				.Build();
			var logfile = new Logfile<StandardLoglevel>();
			await logfile.ReconfigureAsync(logfileConfiguration, default);
			((TestPreprocessor)logfile.Configuration.Preprocessors.OfType<TestPreprocessor>().First()).DisposeCallback =
				() => throw new InvalidOperationException();
			var preprocessorDisposed = false;
			((TestPreprocessor)logfile.Configuration.Preprocessors.OfType<TestPreprocessor>().Last()).DisposeCallback =
				() => preprocessorDisposed = true;
			var newRouter = new TestRouter<LogEvent<StandardLoglevel>>();
			var newLogfileConfiguration = (new LogfileConfigurationBuilder<StandardLoglevel>()
					.AddRouter(newRouter)
					as LogfileConfigurationBuilder<StandardLoglevel>)
				.Build();
			await logfile.ReconfigureAsync(newLogfileConfiguration, default);
			preprocessorDisposed.Should().BeTrue();
			logfile.Configuration.Should().BeSameAs(newLogfileConfiguration);
		}

		[Test]
		public async Task ReconfigureAsyncOnLogfileWithExceptionInRouterDispose_Should_DisposeOtherRoutersAnyway()
		{
			var preprocessor = new TestPreprocessor();
			var router = new TestRouter<LogEvent<StandardLoglevel>>();
			var logfileConfiguration = (new LogfileConfigurationBuilder<StandardLoglevel>()
					.AddPreprocessor(preprocessor)
					.AddRouter(router)
					as LogfileConfigurationBuilder<StandardLoglevel>)
				.Build();
			var logfile = new Logfile<StandardLoglevel>();
			await logfile.ReconfigureAsync(logfileConfiguration, default);
			((TestRouter<LogEvent<StandardLoglevel>>)logfile.Configuration.Routers.First()).DisposeCallback =
					() => throw new InvalidOperationException();
			var routerDisposed = false;
			((TestRouter<LogEvent<StandardLoglevel>>)logfile.Configuration.Routers.Last()).DisposeCallback =
					() => routerDisposed = true;
			var newRouter = new TestRouter<LogEvent<StandardLoglevel>>();
			var newLogfileConfiguration = (new LogfileConfigurationBuilder<StandardLoglevel>()
					.AddRouter(newRouter)
					as LogfileConfigurationBuilder<StandardLoglevel>)
				.Build();
			await logfile.ReconfigureAsync(newLogfileConfiguration, default);
			routerDisposed.Should().BeTrue();
			logfile.Configuration.Should().BeSameAs(newLogfileConfiguration);
		}

		#endregion

		#region New

		[Test]
		public void NewLogEventOnUnconfiguredInstance_ShouldReturn_NewLogEvent()
		{
			// Arrange
			// Act
			var logEvent = new Logfile<StandardLoglevel>().New(StandardLoglevel.Warning);

			// Assert
			logEvent.Loglevel.Should().Be(StandardLoglevel.Warning);
		}

		#endregion

		#region Log

		[Test]
		public void LogNull_Should_Ignore()
		{
			this.prepare(out var logfile, out var router);

			var forwarded = new ManualResetEventSlim();
			router.ForwardCallback = (_logEvents) =>
			{
				forwarded.Set();
			};

			logfile.Log(null);

			forwarded.Wait(TimeSpan.Zero).Should().BeFalse();
		}

		[Test]
		public void LogEmptyListOfEvents_Should_Ignore()
		{
			this.prepare(out var logfile, out var router);

			var forwarded = new ManualResetEventSlim();
			router.ForwardCallback = (_logEvents) =>
			{
				forwarded.Set();
			};

			logfile.Log(new LogEvent<StandardLoglevel>[0]);

			forwarded.Wait(TimeSpan.FromMilliseconds(500)).Should().BeFalse();
		}

		[Test]
		public void LogSingleEvent_Should_ForwardEvent()
		{
			this.prepare(out var logfile, out var router);

			var forwarded = new ManualResetEventSlim();
			LogEvent<StandardLoglevel> forwardedEvent = null;
			router.ForwardCallback = (_logEvents) =>
			{
				forwardedEvent = _logEvents.Single();
				forwarded.Set();
			};
			var event1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			logfile.Log(new LogEvent<StandardLoglevel>[] { event1 });

			forwarded.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
			forwardedEvent.Should().BeSameAs(event1);
		}

		[Test]
		public void LogMultipleEvents_Should_ForwardEvents()
		{
			this.prepare(out var logfile, out var router);

			var forwarded = new ManualResetEventSlim();
			IEnumerable<LogEvent<StandardLoglevel>> forwardedEvents = null;
			router.ForwardCallback = (_logEvents) =>
			{
				forwardedEvents = _logEvents;
				forwarded.Set();
			};
			var event1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var event2 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Error);
			var event3 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Critical);

			logfile.Log(new LogEvent<StandardLoglevel>[] { event1, event2, event3 });

			forwarded.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
			forwardedEvents.Should().Contain(event1);
			forwardedEvents.Should().Contain(event2);
			forwardedEvents.Should().Contain(event3);
		}

		[Test]
		public void LogDeveloperEventWhileEnabled_Should_ForwardEvent()
		{
			var router = new TestRouter<LogEvent<StandardLoglevel>>();
			var logfileConfiguration = (new LogfileConfigurationBuilder<StandardLoglevel>()
					.EnableDeveloperMode()
					.AddRouter(router)
					as LogfileConfigurationBuilder<StandardLoglevel>)
				.Build();
			var logfile = new Logfile<StandardLoglevel>();
			logfile.ReconfigureAsync(logfileConfiguration, default).GetAwaiter().GetResult();

			var forwarded = new ManualResetEventSlim();
			LogEvent<StandardLoglevel> forwardedEvent = null;
			router.ForwardCallback = (_logEvents) =>
			{
				forwardedEvent = _logEvents.Single();
				forwarded.Set();
			};
			var event1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning).Developer;

			logfile.Log(new LogEvent<StandardLoglevel>[] { event1 });

			event1.IsDeveloper.Should().BeTrue();
			forwarded.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
			forwardedEvent.Should().BeSameAs(event1);
		}

		[Test]
		public void LogDeveloperEventWhileNotEnabled_Should_IgnoreEvent()
		{
			this.prepare(out var logfile, out var router);

			var forwarded = new ManualResetEventSlim();
			router.ForwardCallback = (_logEvents) =>
			{
				forwarded.Set();
			};
			var event1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning).Developer;

			logfile.Log(new LogEvent<StandardLoglevel>[] { event1 });

			forwarded.Wait(TimeSpan.FromMilliseconds(500)).Should().BeFalse();
		}

		[Test]
		public void LogForcedEvent_Should_ForwardEvent()
		{
			this.prepare(out var logfile, out var router);

			var forwarded = new ManualResetEventSlim();
			LogEvent<StandardLoglevel> forwardedEvent = null;
			router.ForwardCallback = (_logEvents) =>
			{
				forwardedEvent = _logEvents.Single();
				forwarded.Set();
			};
			var event1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning).Force;

			logfile.Log(new LogEvent<StandardLoglevel>[] { event1 });

			forwarded.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
			forwardedEvent.IsForced.Should().BeTrue();
		}

		[Test]
		public void LogForcedDeveloperEventWhileDeveloperModeNotEnabled_Should_ForwardEvent()
		{
			this.prepare(out var logfile, out var router);

			var forwarded = new ManualResetEventSlim();
			LogEvent<StandardLoglevel> forwardedEvent = null;
			router.ForwardCallback = (_logEvents) =>
			{
				forwardedEvent = _logEvents.Single();
				forwarded.Set();
			};
			var event1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning).Developer.Force;

			logfile.Log(new LogEvent<StandardLoglevel>[] { event1 });

			forwarded.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
			forwardedEvent.IsForced.Should().BeTrue();
		}

		#endregion

		#region Clone

		[Test]
		public void ClonedLogfile_Should_ForwardEvent()
		{
			this.prepare(out var logfile, out var router);

			var forwarded = new ManualResetEventSlim();
			LogEvent<StandardLoglevel> forwardedEvent = null;
			router.ForwardCallback = (_logEvents) =>
			{
				forwardedEvent = _logEvents.Single();
				forwarded.Set();
			};
			var event1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			var proxy = logfile.Clone();
			proxy.Log(new LogEvent<StandardLoglevel>[] { event1 });

			forwarded.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
			forwardedEvent.Should().BeSameAs(event1);
		}

		#endregion
	}
}
