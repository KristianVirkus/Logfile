using EventRouter.Core;
using FluentAssertions;
using Logfile.Core.Preprocessors;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logfile.Core.UnitTests
{
	class LogfileConfigurationBuilderTest
	{
		#region Constructors

		[Test]
		public void Constructor_Should_SetProperties()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			builder.Routers.Should().NotBeNull();
			builder.Preprocessors.Should().NotBeNull();
			builder.MaximumRoutableQueueLength.Should().NotBe(0);
			builder.MaximumRoutablesForwardingCount.Should().NotBe(0);
			builder.WaitForMoreRoutablesForwardingDelay.Should().NotBe(TimeSpan.Zero);
			builder.AllowLoglevels.Should().BeEmpty();
			builder.BlockLoglevels.Should().BeEmpty();
		}

		#endregion

		#region Build

		[Test]
		public void Build_Should_CreateHubConfiguration()
		{
			var router1 = new TestRouter<LogEvent<StandardLoglevel>>();
			var preprocessor1 = new TestPreprocessor();
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			builder.AllowLoglevels.Add(StandardLoglevel.Warning);
			builder.BlockLoglevels.Add(StandardLoglevel.Error);
			builder.Routers.Add(router1);
			builder.Preprocessors.Add(preprocessor1);
			builder.MaximumRoutableQueueLength = 1;
			builder.MaximumRoutablesForwardingCount = 2;
			builder.WaitForMoreRoutablesForwardingDelay = TimeSpan.FromMilliseconds(3);

			var logfileConfiguration = builder.Build();

			logfileConfiguration.Routers.Should().Contain(router1);
			logfileConfiguration.Preprocessors.Should().Contain(preprocessor1);
			logfileConfiguration.MaximumRoutablesQueueLength.Should().Be(1);
			logfileConfiguration.MaximumRoutablesForwardingCount.Should().Be(2);
			logfileConfiguration.WaitForMoreRoutablesForwardingDelay.Should().Be(TimeSpan.FromMilliseconds(3));
			logfileConfiguration.Preprocessors.OfType<LoglevelFilter<StandardLoglevel>>().Single();
		}

		[Test]
		public void BuildWithAllowedAndBlockedLoglevels_Should_CreatePreprocessor()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			builder.AllowLoglevels.Add(StandardLoglevel.Warning);
			builder.AllowLoglevels.Add(StandardLoglevel.Error);
			builder.AllowLoglevels.Add(StandardLoglevel.Critical);
			builder.BlockLoglevels.Add(StandardLoglevel.Error);

			var logfileConfiguration = builder.Build();

			var loglevelFilter = (LoglevelFilter<StandardLoglevel>)logfileConfiguration.Preprocessors.Single();
			loglevelFilter.Process(new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Warning)).Should().BeNull();
			loglevelFilter.Process(new LogEvent<StandardLoglevel>((_logEvent) => { }, StandardLoglevel.Error)).Should().BeEmpty();
		}

		#endregion

		#region Extension methods

		[Test]
		public void AddRouterSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.AddRouter(null, Mock.Of<IRouter<LogEvent<StandardLoglevel>>>()));
		}

		[Test]
		public void AddRouterNull_ShouldThrow_ArgumentNullException()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.AddRouter(builder, null));
		}

		[Test]
		public void AddRouter_Should_AddRouter()
		{
			var router = new TestRouter<LogEvent<StandardLoglevel>>();
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.AddRouter(builder, router);
			builder.Routers.Single().Should().BeSameAs(router);
		}

		[Test]
		public void AddPreprocessorSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.AddPreprocessor(null, Mock.Of<IRoutablePreprocessor<LogEvent<StandardLoglevel>>>()));
		}

		[Test]
		public void AddPreprocessorNull_ShouldThrow_ArgumentNullException()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.AddPreprocessor(builder, null));
		}

		[Test]
		public void AddPreprocessor_Should_Succeed()
		{
			var preprocessor1 = Mock.Of<IRoutablePreprocessor<LogEvent<StandardLoglevel>>>();
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.AddPreprocessor<StandardLoglevel>(builder, preprocessor1);
			builder.Preprocessors.Should().Contain(preprocessor1);
		}

		[Test]
		public void SetMaximumRoutableQueueLengthSelfNull_ShouldThrows_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.SetMaximumRoutableQueueLength<StandardLoglevel>(null, 1));
		}

		[Test]
		public void SetMaximumRoutableQueueLength_Should_SetProperty()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.SetMaximumRoutableQueueLength<StandardLoglevel>(builder, 1);
			builder.MaximumRoutableQueueLength.Should().Be(1);
		}

		[Test]
		public void SetMaximumRoutablesForwardingCountSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.SetMaximumRoutablesForwardingCount<StandardLoglevel>(null, 1));
		}

		[Test]
		public void SetMaximumRoutablesForwardingCount_ShouldThrows_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.SetMaximumRoutablesForwardingCount<StandardLoglevel>(null, 1));
		}

		[Test]
		public void SetMaximumRoutablesForwardingCount_Should_SetProperty()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.SetMaximumRoutablesForwardingCount<StandardLoglevel>(builder, 1);
			builder.MaximumRoutablesForwardingCount.Should().Be(1);
		}

		[Test]
		public void SetWaitForMoreRoutablesForwardingDelaySelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.SetWaitForMoreRoutablesForwardingDelay<StandardLoglevel>(null, TimeSpan.FromSeconds(1)));
		}

		[Test]
		public void SetWaitForMoreRoutablesForwardingDelay_Should_SetProperty()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.SetWaitForMoreRoutablesForwardingDelay<StandardLoglevel>(builder, TimeSpan.FromMilliseconds(1));
			builder.WaitForMoreRoutablesForwardingDelay.Should().Be(TimeSpan.FromMilliseconds(1));
		}

		[Test]
		public void AllowLoglevelSingleSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.AllowLoglevel<StandardLoglevel>(null, StandardLoglevel.Warning));
		}

		[Test]
		public void AllowLoglevelSingle_Should_AddLoglevel()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.AllowLoglevel<StandardLoglevel>(builder, StandardLoglevel.Warning);
			builder.AllowLoglevels.Count.Should().Be(1);
			builder.AllowLoglevels.Should().Contain(StandardLoglevel.Warning);
		}

		[Test]
		public void AllowLoglevelsFromToSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.AllowLoglevels<StandardLoglevel>(null, StandardLoglevel.Warning, StandardLoglevel.Error));
		}

		[Test]
		public void AllowLoglevelsFromTo_Should_AddLoglevels()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.AllowLoglevels<StandardLoglevel>(builder, StandardLoglevel.Warning, StandardLoglevel.Error);
			builder.AllowLoglevels.Count.Should().Be(2);
			builder.AllowLoglevels.Should().Contain(StandardLoglevel.Warning);
			builder.AllowLoglevels.Should().Contain(StandardLoglevel.Error);
		}

		[Test]
		public void AllowLoglevelsFromToInverted_Should_AddLoglevel()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.BlockLoglevels<StandardLoglevel>(builder, StandardLoglevel.Error, StandardLoglevel.Warning);
			builder.BlockLoglevels.Count.Should().Be(2);
			builder.BlockLoglevels.Should().Contain(StandardLoglevel.Warning);
			builder.BlockLoglevels.Should().Contain(StandardLoglevel.Error);
		}

		[Test]
		public void BlockLoglevelSingleSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.BlockLoglevel<StandardLoglevel>(null, StandardLoglevel.Warning));
		}

		[Test]
		public void BlockLoglevelSingle_Should_AddLoglevel()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.BlockLoglevel<StandardLoglevel>(builder, StandardLoglevel.Warning);
			builder.BlockLoglevels.Count.Should().Be(1);
			builder.BlockLoglevels.Should().Contain(StandardLoglevel.Warning);
		}

		[Test]
		public void BlockLoglevelFromToSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.BlockLoglevels<StandardLoglevel>(null, StandardLoglevel.Warning, StandardLoglevel.Error));
		}

		[Test]
		public void BlockLoglevelsFromTo_Should_AddLoglevel()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.BlockLoglevels<StandardLoglevel>(builder, StandardLoglevel.Error, StandardLoglevel.Warning);
			builder.BlockLoglevels.Count.Should().Be(2);
			builder.BlockLoglevels.Should().Contain(StandardLoglevel.Warning);
			builder.BlockLoglevels.Should().Contain(StandardLoglevel.Error);
		}

		[Test]
		public void BlockLoglevelsFromToInverted_Should_AddLoglevel()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.BlockLoglevels<StandardLoglevel>(builder, StandardLoglevel.Warning, StandardLoglevel.Error);
			builder.BlockLoglevels.Count.Should().Be(2);
			builder.BlockLoglevels.Should().Contain(StandardLoglevel.Warning);
			builder.BlockLoglevels.Should().Contain(StandardLoglevel.Error);
		}

		[Test]
		public void EnableDeveloperModeSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.EnableDeveloperMode<StandardLoglevel>(null));
		}

		[Test]
		public void EnableDeveloperMode_Should_EnableDeveloperMode()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.EnableDeveloperMode<StandardLoglevel>(builder);
			builder.IsDeveloperModeEnabled.Should().BeTrue();
		}

#if DEBUG
		[Test]
		public void EnableDeveloperModeForDebugCompilerFlagSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.EnableDeveloperModeForDebugCompilerFlag<StandardLoglevel>(null));
		}

		[Test]
		public void EnableDeveloperModeForDebugCompilerFlag_Should_EnableDeveloperMode()
		{
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();
			LogfileConfigurationBuilderExtensions.EnableDeveloperModeForDebugCompilerFlag<StandardLoglevel>(builder);
			builder.IsDeveloperModeEnabled.Should().BeTrue();
		}
#endif

		[Test]
		public void UseLogEventsFromExceptionDataSelfNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => LogfileConfigurationBuilderExtensions.UseLogEventsFromExceptionData<StandardLoglevel>(null));
		}

		[Test]
		public void UseLogEventsFromExceptionData_Should_AddPreprocessor()
		{
			// Arrange
			var builder = new LogfileConfigurationBuilder<StandardLoglevel>();

			// Act
			LogfileConfigurationBuilderExtensions.UseLogEventsFromExceptionData<StandardLoglevel>(builder);

			// Assert
			builder.Preprocessors.OfType<ExtractLogEventsFromExceptions<StandardLoglevel>>().Count().Should().Be(1);
		}

		#endregion
	}
}
