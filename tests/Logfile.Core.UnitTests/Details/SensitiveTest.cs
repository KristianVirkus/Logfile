using FluentAssertions;
using Logfile.Core.Details;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Logfile.Core.UnitTests.Details
{
	class SensitiveTest
	{
		static Sensitive createSensitive(bool isSensitive = true, string setupName = null)
			=> new Sensitive(isSensitive, setupName);

		[Test]
		public void SensitiveWithSetupNull_Should_UseRandomSalt()
		{
			var sensitive = createSensitive(isSensitive: true, setupName: null);
			sensitive.IsSensitive.Should().BeTrue();
			sensitive.SetupName.Should().BeNull();
			sensitive.ToString().Should().Be("Sensitive");
		}

		[Test]
		public void SensitiveWithSetupName_Should_UseSetupName()
		{
			var sensitive = createSensitive(isSensitive: true, setupName: "test");
			sensitive.IsSensitive.Should().BeTrue();
			sensitive.SetupName.Should().Be("test");
			sensitive.ToString().Should().Be("Sensitive (Setup=test)");
		}

		[Test]
		public void InsensitiveWithSetupName_ShouldThrow_ArgumentException()
		{
			Assert.Throws<ArgumentException>(() => createSensitive(isSensitive: false, setupName: "test"));
		}

		[Test]
		public void InsensitiveWithoutSetupName_Should_UseNoSalt()
		{
			var sensitive = createSensitive(false, null);
			sensitive.IsSensitive.Should().BeFalse();
			sensitive.SetupName.Should().BeNull();
			sensitive.ToString().Should().Be("Insensitive");
		}

		[Test]
		public void AddSensitiveWithoutSetupName_Should_AddSensitiveDetail()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			SensitiveExtensions.Sensitive<StandardLoglevel>(logEvent).Should().BeSameAs(logEvent);

			var sensitive = new Sensitive(true, null);
			sensitive.IsSensitive.Should().BeTrue();
			sensitive.SetupName.Should().BeNull();
			sensitive.ToString().Should().StartWith("Sensitive");
		}

		[Test]
		public void AddSensitiveWithSetupName_Should_AddSensitiveDetail()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			SensitiveExtensions.Sensitive<StandardLoglevel>(logEvent, "test").Should().BeSameAs(logEvent);

			var sensitive = logEvent.Details.OfType<Sensitive>().Single();
			sensitive.IsSensitive.Should().BeTrue();
			sensitive.SetupName.SequenceEqual("test");
			sensitive.ToString().Should().Be("Sensitive (Setup=test)");
		}

		[Test]
		public void AddInsensitive_Should_AddSensitiveDetail()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			SensitiveExtensions.Insensitive<StandardLoglevel>(logEvent).Should().BeSameAs(logEvent);

			var sensitive = logEvent.Details.OfType<Sensitive>().Single();
			sensitive.IsSensitive.Should().BeFalse();
			sensitive.SetupName.Should().BeNull();
			sensitive.ToString().Should().Be("Insensitive");
		}

		[Test]
		public void EncryptWithSensitiveSettingsNull_ShouldThrow_ArgumentNullException()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var sensitive = SensitiveExtensions.Sensitive<StandardLoglevel>(logEvent);

			Assert.Throws<ArgumentNullException>(() => sensitive.Details
														.OfType<Sensitive>().Single()
														.Encrypt<StandardLoglevel>(null, Encoding.UTF8.GetBytes("Test")));
		}

		[Test]
		public void EncryptWithDataNull_ShouldReturn_Null()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var sensitive = SensitiveExtensions.Sensitive<StandardLoglevel>(logEvent);

			var sensitiveSettings = new Aes256SensitiveSettings(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("secret")));
			var cipher = sensitive.Details
							.OfType<Sensitive>().Single()
							.Encrypt<StandardLoglevel>(sensitiveSettings, null);
			cipher.Should().BeNull();
		}

		[Test]
		public void EncryptWithUnsupportedSensitiveSettings_ShouldThrow_NotSupportedException()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var sensitive = SensitiveExtensions.Sensitive<StandardLoglevel>(logEvent);

			Assert.Throws<NotSupportedException>(() => sensitive.Details
														.OfType<Sensitive>().Single()
														.Encrypt<StandardLoglevel>(Mock.Of<ISensitiveSettings>(), Encoding.UTF8.GetBytes("Test")));
		}

		[Test]
		public void Encrypt_Should_Succeed()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var sensitive = SensitiveExtensions.Sensitive<StandardLoglevel>(logEvent);

			var sensitiveSettings = new Aes256SensitiveSettings(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("secret")));
			var cipher = sensitive.Details
							.OfType<Sensitive>().Single()
							.Encrypt<StandardLoglevel>(sensitiveSettings, Encoding.UTF8.GetBytes("Test"));
			cipher.Should().NotBeEmpty();
		}

		[Test]
		public void SerializeCipherNull_ShouldThrow_ArgumentNullException()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var sensitive = SensitiveExtensions.Sensitive<StandardLoglevel>(logEvent);

			Assert.Throws<ArgumentNullException>(() => sensitive.Details
														.OfType<Sensitive>().Single()
														.Serialize(null));
		}

		[Test]
		public void SerializeCipher_Should_Succeed()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var sensitive = SensitiveExtensions.Sensitive<StandardLoglevel>(logEvent);

			sensitive.Details
				.OfType<Sensitive>().Single()
				.Serialize(new byte[] { 0x00, 0x01, 0x02, 0x03 }).Should().NotBeEmpty();
		}

		[Test]
		public void ExtensionSensitiveLogEventNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => SensitiveExtensions.Sensitive<StandardLoglevel>(null));
		}

		[Test]
		public void ExtensionSensitive_Should_AddDetail()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var sensitive = SensitiveExtensions.Sensitive<StandardLoglevel>(logEvent, setupName: "test");

			sensitive.Details.OfType<Sensitive>().Single().IsSensitive.Should().BeTrue();
			sensitive.Details.OfType<Sensitive>().Single().SetupName.Should().Be("test");
		}


		[Test]
		public void ExtensionInsensitiveLogEventNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => SensitiveExtensions.Insensitive<StandardLoglevel>(null));
		}

		[Test]
		public void ExtensionInsensitive_Should_AddDetail()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var sensitive = SensitiveExtensions.Insensitive<StandardLoglevel>(logEvent);

			sensitive.Details.OfType<Sensitive>().Single().IsSensitive.Should().BeFalse();
		}
	}
}
