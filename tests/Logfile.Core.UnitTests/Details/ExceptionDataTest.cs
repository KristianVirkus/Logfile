using EventRouter.Core;
using FluentAssertions;
using Logfile.Core.Details;
using Logfile.Core.Preprocessors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Logfile.Core.UnitTests.Details
{
	class ExceptionDataTest
	{
		[Test]
		public void AddLogEventWithExceptionNull_ShouldThrow_ArgumentNullException()
		{
			// Arrange
			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() =>
				ExceptionDataExtensions.AddLogEvent(
					null,
					StandardLoglevel.Warning));
		}

		[Test]
		public void AddLogEvent_Should_CreateLogEventInExceptionDataDictionary()
		{
			// Arrange
			var exception = new InvalidOperationException();

			// Act
			var logEvent = ExceptionDataExtensions.AddLogEvent(
				exception,
				StandardLoglevel.Warning);

			// Assert
			exception.Data[typeof(LogEvent<StandardLoglevel>)].Should().BeSameAs(logEvent);
		}

		[Test]
		public void AddLogEventTwice_Should_OverwriteFirstDataDictionaryInException()
		{
			// Arrange
			var exception = new InvalidOperationException();

			// Act
			var logEvent1 = ExceptionDataExtensions.AddLogEvent(
				exception,
				StandardLoglevel.Warning);
			var logEvent2 = ExceptionDataExtensions.AddLogEvent(
				exception,
				StandardLoglevel.Error);

			// Assert
			exception.Data[typeof(LogEvent<StandardLoglevel>)].Should().BeSameAs(logEvent2);
		}

		[Test]
		public void AddLogEventWithTimeNull_Should_UseCurrentTime()
		{
			// Arrange
			var exception = new InvalidOperationException();

			// Act
			var logEvent = ExceptionDataExtensions.AddLogEvent(
				exception,
				StandardLoglevel.Warning);

			// Assert
			logEvent.Time.Should().BeCloseTo(DateTime.Now);
		}

		[Test]
		public async Task AddLogEvent_Should_LogThisLogEventAutomaticallyAfterTheLogEventContainingTheExceptionDetails()
		{
			// Arrange
			var logfileConfigurationBuilder = new LogfileConfigurationBuilder<StandardLoglevel>();
			logfileConfigurationBuilder
				.AddPreprocessor(ExtractLogEventsFromExceptions<StandardLoglevel>.Instance);

			var logfile = new Logfile<StandardLoglevel>();
			await logfile.ReconfigureAsync(logfileConfigurationBuilder.Build(), default);

			var exception = new InvalidOperationException();

			// Act
			var logEvent = ExceptionDataExtensions.AddLogEvent(
				exception,
				StandardLoglevel.Warning);

			// Assert
			exception.Data[typeof(LogEvent<StandardLoglevel>)].Should().BeSameAs(logEvent);
		}
	}
}
