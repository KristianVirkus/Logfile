using FluentAssertions;
using Logfile.Core.Details;
using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Logfile.Core.UnitTests.Details
{
	class BinaryTest
	{
		[Test]
		public void LogEventNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => BinaryExtensions.Binary<StandardLoglevel>(null, new byte[] { 0x00, 0x01, 0x02, 0x03 }));
		}

		[Test]
		public void DataNull_Should_UseNull()
		{
			var binary = new Binary(null);
			binary.Data.Should().BeNull();
			binary.ToString().Should().Be("null");
		}

		[Test]
		public void EmptyData_Should_ReportZeroBytes()
		{
			// Arrange
			var data = new ReadOnlyCollection<byte>(new byte[0]);

			// Act
			var binary = new Binary(data);

			// Assert
			binary.Data.Should().BeSameAs(data);
			binary.ToString().Should().Be("0 Bytes");
		}

		[Test]
		public void OneByteData_Should_ReportNumberOfBytes()
		{
			// Arrange
			var data = new ReadOnlyCollection<byte>(new byte[] { 0x30 });

			// Act
			var binary = new Binary(data);

			// Assert
			binary.Data.Should().BeSameAs(data);
			binary.ToString().Should().Be("1 Byte");
		}

		[Test]
		public void FourBytesData_Should_ReportNumberOfBytes()
		{
			// Arrange
			var data = new ReadOnlyCollection<byte>(new byte[] { 0x00, 0x01, 0x02, 0x03 });

			// Act
			var binary = new Binary(data);

			// Assert
			binary.Data.Should().BeSameAs(data);
			binary.ToString().Should().Be("4 Bytes");
		}

		[Test]
		public void AddBinaryNull_Should_NotAddLogEventDetail()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			BinaryExtensions.Binary<StandardLoglevel>(logEvent, null).Should().BeSameAs(logEvent);
			logEvent.Details.OfType<Binary>().Any().Should().BeFalse();
		}

		[Test]
		public void AddBinaryData_Should_AddLogEventDetail()
		{
			var data = new byte[] { 48, 49, 50, 51 };
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			BinaryExtensions.Binary<StandardLoglevel>(logEvent, data).Should().BeSameAs(logEvent);
			logEvent.Details.OfType<Binary>().Single().Data.SequenceEqual(data).Should().BeTrue();
		}
	}
}
