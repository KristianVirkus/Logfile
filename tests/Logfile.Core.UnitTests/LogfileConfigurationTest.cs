using EventRouter.Core;
using NUnit.Framework;
using FluentAssertions;
using System;

namespace Logfile.Core.UnitTests
{
	public class LogfileConfigurationTest
	{
		[Test]
		public void Constructor_Should_SetProperties()
		{
			var router1 = new TestRouter<LogEvent<StandardLoglevel>>();
			var routers = new IRouter<LogEvent<StandardLoglevel>>[] { router1 };
			var preprocessor1 = new TestPreprocessor();
			var preprocessors = new IRoutablePreprocessor<LogEvent<StandardLoglevel>>[] { preprocessor1 };

			var logfileConfiguration = new LogfileConfiguration<StandardLoglevel>(
				routers,
				preprocessors,
				1,
				2,
				TimeSpan.FromMilliseconds(3),
				true);

			logfileConfiguration.Routers.Should().Contain(router1);
			logfileConfiguration.Preprocessors.Should().Contain(preprocessor1);
			logfileConfiguration.MaximumRoutablesQueueLength.Should().Be(1);
			logfileConfiguration.MaximumRoutablesForwardingCount.Should().Be(2);
			logfileConfiguration.WaitForMoreRoutablesForwardingDelay.Should().Be(TimeSpan.FromMilliseconds(3));
			logfileConfiguration.IsDeveloperModeEnabled.Should().BeTrue();
		}
	}
}