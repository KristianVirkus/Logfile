using FluentAssertions;
using NUnit.Framework;
using System;

namespace Logfile.Core.UnitTests
{
	class LogEventTest
	{
		[Test]
		public void ConstructorLogCallbackNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new LogEvent<StandardLoglevel>(null, StandardLoglevel.Information));
		}

		[Test]
		public void Constructor_Should_SetProperties()
		{
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Error);
			logEvent.Loglevel.Should().Be(StandardLoglevel.Error);
			logEvent.Details.Should().NotBeNull();
		}

		[Test]
		public void Developer_Should_SetProperty()
		{
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Warning);
			var _ = logEvent.Developer;
			logEvent.IsDeveloper.Should().BeTrue();
		}

		[Test]
		public void Force_Should_SetProperty()
		{
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Warning);
			var _ = logEvent.Force;
			logEvent.IsForced.Should().BeTrue();
		}

		[Test]
		public void Log_Should_StoreCallerInformation()
		{
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Warning);
			logEvent.Log();
			logEvent.CallerFilePath.Should().EndWith("LogEventTest.cs");
			logEvent.CallerMemberName.Should().Be("Log_Should_StoreCallerInformation");
			logEvent.CallerLineNumber.Should().BeGreaterThan(1);
		}

		[Test]
		public void Log_Should_InvokeLogCallback()
		{
			LogEvent<StandardLoglevel> logEventArgument = null;
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { logEventArgument = _logEvent; }, StandardLoglevel.Warning);
			logEvent.Log();
			logEventArgument.Should().BeSameAs(logEvent);
		}
	}
}
