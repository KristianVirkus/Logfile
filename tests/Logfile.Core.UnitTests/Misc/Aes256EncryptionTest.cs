using FluentAssertions;
using Logfile.Core.Misc;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logfile.Core.UnitTests.Misc
{
	class Aes256EncryptionTest
	{
		static readonly byte[] KeyDefault = new Guid("{279DF010-E704-41C4-ABF9-C0DC1A42671F}").ToByteArray().Concat(new Guid("{3AB05A55-1BC6-4E24-8F8D-1B99A4A74418}").ToByteArray()).ToArray();
		static readonly byte[] IVDefault = new Guid("{813B0A0D-1DD6-43A6-881C-08E332754E8E}").ToByteArray();

		public class Encrypt
		{
			[Test]
			public void DataNull_ShouldThrow_ArgumentNullException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentNullException>(async () => await Aes256Encryption.Encrypt(null, KeyDefault, IVDefault));
			}

			[Test]
			public void KeyNull_ShouldThrow_ArgumentNullException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentNullException>(async () => await Aes256Encryption.Encrypt(Encoding.UTF8.GetBytes("Top secret"), null, IVDefault));
			}

			[Test]
			public void KeyNot32Bytes_ShouldThrow_ArgumentOutOfRangeException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Aes256Encryption.Encrypt(Encoding.UTF8.GetBytes("Top secret"), new byte[] { 0x00, 0x01 }, IVDefault));
			}

			[Test]
			public void IVNull_ShouldThrow_ArgumentNullException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentNullException>(async () => await Aes256Encryption.Encrypt(Encoding.UTF8.GetBytes("Top secret"), KeyDefault, null));
			}

			[Test]
			public void IVNot16Bytes_ShouldThrow_ArgumentOutOfRangeException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Aes256Encryption.Encrypt(Encoding.UTF8.GetBytes("Top secret"), KeyDefault, new byte[] { 0x00, 0x01 }));
			}

			[Test]
			public async Task DataEmpty_ShouldReturn_EmptyArray()
			{
				// Arrange
				// Act
				var data = await Aes256Encryption.Encrypt(new byte[0], KeyDefault, IVDefault);

				// Assert
				data.Should().BeEmpty();
			}
		}

		public class Decrypt
		{
			[Test]
			public void DataNull_ShouldThrow_ArgumentNullException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentNullException>(async () => await Aes256Encryption.Decrypt(null, KeyDefault, IVDefault));
			}

			[Test]
			public void KeyNull_ShouldThrow_ArgumentNullException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentNullException>(async () => await Aes256Encryption.Decrypt(Encoding.UTF8.GetBytes("Top secret"), null, IVDefault));
			}

			[Test]
			public void KeyNot32Bytes_ShouldThrow_ArgumentOutOfRangeException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Aes256Encryption.Decrypt(Encoding.UTF8.GetBytes("Top secret"), new byte[] { 0x00, 0x01 }, IVDefault));
			}

			[Test]
			public void IVNull_ShouldThrow_ArgumentNullException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentNullException>(async () => await Aes256Encryption.Decrypt(Encoding.UTF8.GetBytes("Top secret"), KeyDefault, null));
			}

			[Test]
			public void IVNot16Bytes_ShouldThrow_ArgumentOutOfRangeException()
			{
				// Arrange
				// Act
				// Assert
				Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Aes256Encryption.Decrypt(Encoding.UTF8.GetBytes("Top secret"), KeyDefault, new byte[] { 0x00, 0x01 }));
			}

			[Test]
			public async Task DataEmpty_ShouldReturn_EmptyArray()
			{
				// Arrange
				// Act
				var encrypted = await Aes256Encryption.Encrypt(new byte[0], KeyDefault, IVDefault);

				// Assert
				encrypted.Should().BeEmpty();
			}

			[Test]
			public async Task Roundtrip_Should_Succeed()
			{
				// Arrange
				var data = Encoding.UTF8.GetBytes("Top secret");

				// Act
				var encrypted = await Aes256Encryption.Encrypt(data, KeyDefault, IVDefault);
				var decrypted = await Aes256Encryption.Decrypt(encrypted, KeyDefault, IVDefault);

				// Assert
				encrypted.SequenceEqual(data).Should().BeFalse();
				decrypted.SequenceEqual(data).Should().BeTrue();
			}
		}
	}
}
