using EventRouter.Core;
using FluentAssertions;
using Logfile.Core.Details;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Logfile.Core.UnitTests
{
	class LogfileProxyTest
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

		[Test]
		public void CloneClone_Should_CreateAnotherClone()
		{
			this.prepare(out var logfile, out var router);
			var proxy = logfile.Clone();
			var proxy2 = proxy.Clone();

			proxy2.Should().NotBeSameAs(proxy);
		}

		[Test]
		public void Clone_Should_ExtendHierarchy()
		{
			this.prepare(out var logfile, out var router);

			string.Join(".", logfile.Clone("proxy1").Clone("proxy2").Clone("proxy3").Hierarchy).Should().Be("proxy1.proxy2.proxy3");
		}

		[Test]
		public void CloneWithoutName_Should_NotExtendHierarchy()
		{
			this.prepare(out var logfile, out var router);

			string.Join(".", logfile.Clone().Clone().Clone("test").Clone().Hierarchy).Should().Be("test");
		}

		[Test]
		public void Log_Should_ForwardLogEvent()
		{
			this.prepare(out var logfile, out var router);
			var forwarded = new ManualResetEventSlim();
			LogEvent<StandardLoglevel> forwardedEvent = null;
			router.ForwardCallback = (_logEvents) => { forwardedEvent = _logEvents.Single(); forwarded.Set(); };
			var proxy = logfile.Clone("proxy1");

			proxy.Log(new LogEvent<StandardLoglevel>[] { new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Critical) });

			forwarded.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
			string.Join(" ", forwardedEvent.Details.OfType<LogfileHierarchy>().Single().Hierarchy).Should().Be("proxy1");
		}
	}
}
