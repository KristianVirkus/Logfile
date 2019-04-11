using FluentAssertions;
using Logfile.Core.Preprocessors;
using NUnit.Framework;
using System;

namespace Logfile.Core.UnitTests
{
	class LoglevelFilterTest
	{
		[Test]
		public void ConstructorAllowLoglevels_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new LoglevelFilter<StandardLoglevel>(null, new StandardLoglevel[0]));
		}

		[Test]
		public void ConstructorBlockLoglevels_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new LoglevelFilter<StandardLoglevel>(new StandardLoglevel[0], null));
		}

		[Test]
		public void Constructor_Should_SetProperties()
		{
			var allowed = new StandardLoglevel[] { StandardLoglevel.Information };
			var blocked = new StandardLoglevel[] { StandardLoglevel.Warning };
			var filter = new LoglevelFilter<StandardLoglevel>(allowed, blocked);
			filter.AllowLoglevels.Should().Contain(StandardLoglevel.Information);
			filter.BlockLoglevels.Should().Contain(StandardLoglevel.Warning);
		}

		[Test]
		public void ProcessNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				new LoglevelFilter<StandardLoglevel>(new StandardLoglevel[0], new StandardLoglevel[0]).Process(null));
		}

		[Test]
		public void ProcessAllowedLoglevel_ShouldReturn_Null()
		{
			var filter = new LoglevelFilter<StandardLoglevel>(new StandardLoglevel[] { StandardLoglevel.Information }, new StandardLoglevel[0]);
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Information);
			filter.Process(logEvent).Should().BeNull();
		}

		[Test]
		public void ProcessBlockedLogLevel_ShouldReturn_EmptyList()
		{
			var filter = new LoglevelFilter<StandardLoglevel>(new StandardLoglevel[0], new StandardLoglevel[] { StandardLoglevel.Information });
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Information);
			filter.Process(logEvent).Should().BeEmpty();
		}

		[Test]
		public void ProcessRoutableWithNoLoglevelsConfigured_ShouldReturn_Null()
		{
			var filter = new LoglevelFilter<StandardLoglevel>(new StandardLoglevel[0], new StandardLoglevel[0]);
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Information);
			filter.Process(logEvent).Should().BeNull();
		}

		[Test]
		public void ProcessRoutableWithinAllowedAndBlockedLoglevels_ShouldReturn_EmptyList()
		{
			var filter = new LoglevelFilter<StandardLoglevel>(new StandardLoglevel[] { StandardLoglevel.Information }, new StandardLoglevel[] { StandardLoglevel.Information });
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Information);
			filter.Process(logEvent).Should().BeEmpty();
		}

		[Test]
		public void ProcessForcedRoutableWithLoglevelOutOfRange_ShouldReturn_Null()
		{
			var filter = new LoglevelFilter<StandardLoglevel>(new StandardLoglevel[0], new StandardLoglevel[] { StandardLoglevel.Trace });
			var logEvent = new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Trace).Force;
			filter.Process(logEvent).Should().BeNull();
		}
	}
}
