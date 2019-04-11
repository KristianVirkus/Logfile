using System;
using System.Collections.Generic;
using System.Linq;

namespace Logfile.Core.Details
{
	/// <summary>
	/// Represents sensitive settings log event details for AES256 encryption.
	/// </summary>
	public class Aes256SensitiveSettings : ISensitiveSettings
	{
		/// <summary>
		/// Gets the initialization vector.
		/// </summary>
		public static readonly byte[] IV = new Guid("{7DB5A851-D8B9-470F-B06E-537745E92330}").ToByteArray();

		/// <summary>
		/// Gets the AES secret.
		/// </summary>
		public byte[] Secret { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Aes256SensitiveSettings"/> class.
		/// </summary>
		/// <param name="secret">The 256 bits/32 bytes secret.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="secret"/>
		///		is null.</exception>
		///	<exception cref="ArgumentOutOfRangeException">Thrown if the length of
		///		<paramref name="secret"/> is not 256 bits/32 bytes.</exception>
		public Aes256SensitiveSettings(IReadOnlyCollection<byte> secret)
		{
			this.Secret = secret?.ToArray() ?? throw new ArgumentNullException(nameof(secret));
			if (secret.Count != 32) throw new ArgumentOutOfRangeException(nameof(secret));
		}
	}
}
