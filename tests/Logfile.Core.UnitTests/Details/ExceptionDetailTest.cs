using EventRouter.Core;
using FluentAssertions;
using Logfile.Core.Details;
using Logfile.Core.Preprocessors;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Logfile.Core.UnitTests.Details
{
	class ExceptionDetailTest
	{
		[Test]
		public void ExceptionNull_Should_SetExceptionPropertyNull()
		{
			// Arrange
			// Act
			var obj = new ExceptionDetail(null);

			// Assert
			obj.ExceptionObject.Should().BeNull();
		}

		[Test]
		public void ExceptionNullToString_ShouldReturn_NullText()
		{
			// Arrange
			// Act
			var obj = new ExceptionDetail(null);

			// Assert
			obj.ToString().Should().Be("null");
		}

		[Test]
		public void Constructor_Should_SetProperties()
		{
			// Arrange
			var exception = new InvalidOperationException();

			// Act
			var obj = new ExceptionDetail(exception);

			// Assert
			obj.ExceptionObject.Should().BeSameAs(exception);
		}

		[Test]
		public void ToString_ShouldReturn_SerializedException()
		{
			// Arrange
			var exception = new InvalidOperationException();

			// Act
			var obj = new ExceptionDetail(exception);

			// Assert
			obj.ToString().Should().Be(exception.ToString());
		}

		[Test]
		public void AddExceptionWithLogEventNull_ShouldThrow_ArgumentNullException()
		{
			// Arrange
			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => ExceptionExtensions.Exception<StandardLoglevel>(null, new InvalidOperationException()));
		}

		[Test]
		public void AddExceptionNull_Should_NotAddDetails()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			// Act
			ExceptionExtensions.Exception(logEvent, null).Should().BeSameAs(logEvent);

			// Assert
			logEvent.Details.OfType<ExceptionDetail>().Should().BeEmpty();
		}

		[Test]
		public void AddException_Should_AddDetails()
		{
			// Arrange
			var exception = new InvalidOperationException();
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			// Act
			ExceptionExtensions.Exception(logEvent, exception).Should().BeSameAs(logEvent);

			// Assert
			logEvent.Details.OfType<ExceptionDetail>().Count().Should().Be(1);
		}
	}
}
