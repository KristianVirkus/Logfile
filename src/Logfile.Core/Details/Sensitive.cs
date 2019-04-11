using Logfile.Core.Misc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Logfile.Core.Details
{
	/// <summary>
	/// Represents beginning or ending of sensitive information log event details.
	/// </summary>
	public class Sensitive
	{
		/// <summary>
		/// Gets whether to begin or end sensitive data within the log event.
		/// </summary>
		public bool IsSensitive { get; }

		/// <summary>
		/// Gets the name of the setup to use.
		/// </summary>
		public string SetupName { get; }

		/// <summary>
		/// Initialize a new instance of the <see cref="Sensitive"/> class.
		/// </summary>
		/// <param name="isSensitive">true to begin secure data, false to
		///		end secure data within the log event.</param>
		///	<param name="setupName">The name of the setup to use.</param>
		///	<exception cref="ArgumentException">Thrown if <paramref name="setupName"/>
		///		is null, but <paramref name="isSensitive"/> is false.</exception>
		public Sensitive(bool isSensitive, string setupName)
		{
			if ((!isSensitive) && (setupName != null))
				throw new ArgumentException($"The setup name must not be specified together with {nameof(isSensitive)}.", nameof(setupName));

			this.IsSensitive = isSensitive;
			this.SetupName = setupName;
		}

		public override string ToString() => $"{(this.IsSensitive ? $"Sensitive{(this.SetupName == null ? "" : $" (Setup={this.SetupName})")}" : "Insensitive")}";

		/// <summary>
		/// Encrypts <paramref name="data"/>.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="settings">The sensitive settings.</param>
		/// <param name="data">The data.</param>
		/// <returns>The cipher from <paramref name="data"/>, or null if
		///		<paramref name="data"/> is null.</returns>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="settings"/> is null.</exception>
		/// <exception cref="NotSupportedException">Thrown if the
		///		<paramref name="settings"/> type is not supported.</exception>
		public byte[] Encrypt<TLoglevel>(ISensitiveSettings settings, byte[] data)
			where TLoglevel : Enum
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			switch (settings)
			{
				case Aes256SensitiveSettings aes256Settings:
					if (data == null) return null;
					return Aes256Encryption.Encrypt(data, aes256Settings.Secret, Aes256SensitiveSettings.IV).GetAwaiter().GetResult();
				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Serializes a <paramref name="cipher"/>.
		/// </summary>
		/// <param name="cipher">The cipher.</param>
		/// <returns>The serialized cipher.</returns>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="cipher"/> is null.</exception>
		public string Serialize(byte[] cipher)
		{
			if (cipher == null) throw new ArgumentNullException(nameof(cipher));
			return Convert.ToBase64String(cipher);
		}
	}

	/// <summary>
	/// Implements extension methods for fluent event creation.
	/// </summary>
	public static class SensitiveExtensions
	{
		/// <summary>
		/// Begins sensitive data.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="logEvent">The log event.</param>
		/// <returns><paramref name="logEvent"/></returns>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logEvent"/> is null.</exception>
		public static LogEvent<TLoglevel> Sensitive<TLoglevel>(this LogEvent<TLoglevel> logEvent,
			string setupName = null)
			where TLoglevel : Enum
		{
			if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
			logEvent.Details.Add(new Sensitive(true, setupName));
			return logEvent;
		}

		/// <summary>
		/// Ends sensitive data.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="logEvent">The log event.</param>
		/// <returns><paramref name="logEvent"/></returns>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logEvent"/> is null.</exception>
		public static LogEvent<TLoglevel> Insensitive<TLoglevel>(this LogEvent<TLoglevel> logEvent)
			where TLoglevel : Enum
		{
			if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
			logEvent.Details.Add(new Sensitive(false, null));
			return logEvent;
		}
	}
}
