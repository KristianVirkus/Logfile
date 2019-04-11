using FluentAssertions;
using Logfile.Core.Details;
using Logfile.Core.Preprocessors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logfile.Core.UnitTests.Preprocessors
{
	class ExtractLogEventsFromExceptionsTest
	{
		[Test]
		public void ProcessLogEventNull_ShouldThrow_ArgumentNullException()
		{
			// Arrange
			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => new ExtractLogEventsFromExceptions<StandardLoglevel>().Process(null));
		}

		[Test]
		public void ProcessLogEventWithoutExceptionDetails_Should_KeepOriginalLogEvent()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var preprocessor = new ExtractLogEventsFromExceptions<StandardLoglevel>();

			// Act
			var results = preprocessor.Process(logEvent);

			// Assert
			results.Single().Should().BeSameAs(logEvent);
		}

		[Test]
		public void ProcessLogEventWithExceptionDetailsButWithoutData_Should_KeepOriginalLogEvent()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			logEvent.Details.Add(new object());
			var preprocessor = new ExtractLogEventsFromExceptions<StandardLoglevel>();

			// Act
			var results = preprocessor.Process(logEvent);

			// Assert
			results.Single().Should().BeSameAs(logEvent);
		}

		[Test]
		public void ProcessLogEventWithExceptionDetailsWithLogEventDataNull_Should_KeepOriginalLogEventButIgnoreExceptionData()
		{
			// Arrange
			var exception = new InvalidOperationException();
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning).Exception(exception);
			var preprocessor = new ExtractLogEventsFromExceptions<StandardLoglevel>();

			// Act
			var results = preprocessor.Process(logEvent);

			// Assert
			results.Single().Should().BeSameAs(logEvent);
		}

		[Test]
		public void ProcessLogEventWithExceptionDetailsButIncompatibleExceptionDataType_Should_KeepOriginalLogEventButIgnoreExceptionData()
		{
			// Arrange
			var exception = new InvalidOperationException();
			exception.Data[typeof(LogEvent<StandardLoglevel>)] = "invalid value type";
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning).Exception(exception);
			var preprocessor = new ExtractLogEventsFromExceptions<StandardLoglevel>();

			// Act
			var results = preprocessor.Process(logEvent);

			// Assert
			results.Single().Should().BeSameAs(logEvent);
		}

		[Test]
		public void ProcessLogEventWithExceptionDetailsWithEmptyLogEventData_Should_KeepOriginalLogEvent()
		{
			// Arrange
			var exception = new InvalidOperationException();
			exception.Data[typeof(LogEvent<StandardLoglevel>)] = new List<LogEvent<StandardLoglevel>>();
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning).Exception(exception);
			var preprocessor = new ExtractLogEventsFromExceptions<StandardLoglevel>();

			// Act
			var results = preprocessor.Process(logEvent);

			// Assert
			results.Single().Should().BeSameAs(logEvent);
		}

		[Test]
		public void ProcessLogEventWithExceptionDetailsWithAdditionalLogEventData_Should_KeepOriginalLogEventAndAppendAdditionalOnes()
		{
			// Arrange
			var nestedLogEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Error);
			var exception = new InvalidOperationException();
			exception.Data[typeof(LogEvent<StandardLoglevel>)] = new List<LogEvent<StandardLoglevel>> { nestedLogEvent };
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning).Exception(exception);
			var preprocessor = new ExtractLogEventsFromExceptions<StandardLoglevel>();

			// Act
			var results = preprocessor.Process(logEvent);

			// Assert
			results.Count().Should().Be(2);
			results.First().Should().BeSameAs(logEvent);
			results.Last().Should().BeSameAs(nestedLogEvent);
		}

		[Test]
		public void ProcessLogEventWithExceptionDetailsWithAdditionalLogEventDataWithNestedExceptionLogEventData_Should_KeepOriginalLogEventAndAppendAllAdditionalOnesRecursively()
		{
			// Arrange
			var nestedLogEvent1_1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Critical);
			var nestedLogEvent1_2 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Debug);
			var nestedException1 = new InvalidOperationException();
			nestedException1.Data[typeof(LogEvent<StandardLoglevel>)] = new List<LogEvent<StandardLoglevel>> { nestedLogEvent1_1, nestedLogEvent1_2 };
			var nestedLogEvent1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Trace).Exception(nestedException1);
			var nestedLogEvent2_1 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Error);
			var nestedException2 = new InvalidOperationException();
			nestedException2.Data[typeof(LogEvent<StandardLoglevel>)] = new List<LogEvent<StandardLoglevel>> { nestedLogEvent2_1 };
			var nestedLogEvent2 = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Error).Exception(nestedException2);
			var exception = new InvalidOperationException();
			exception.Data[typeof(LogEvent<StandardLoglevel>)] = new List<LogEvent<StandardLoglevel>> { nestedLogEvent1, nestedLogEvent2 };
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning).Exception(exception);
			var preprocessor = new ExtractLogEventsFromExceptions<StandardLoglevel>();

			// Act
			var results = preprocessor.Process(logEvent);

			// Assert
			results.Count().Should().Be(6);
			results.ElementAt(0).Should().BeSameAs(logEvent);
			results.ElementAt(1).Should().BeSameAs(nestedLogEvent1);
			results.ElementAt(2).Should().BeSameAs(nestedLogEvent1_1);
			results.ElementAt(3).Should().BeSameAs(nestedLogEvent1_2);
			results.ElementAt(4).Should().BeSameAs(nestedLogEvent2);
			results.ElementAt(5).Should().BeSameAs(nestedLogEvent2_1);
		}
	}
}
