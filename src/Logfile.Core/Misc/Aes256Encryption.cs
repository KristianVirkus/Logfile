using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Logfile.Core.Misc
{
	/// <summary>
	/// Implements helpers to perform AES256 encryption and decryption.
	/// </summary>
	public static class Aes256Encryption
	{
		/// <summary>
		/// Encrypts <paramref name="data"/>.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="key">The encryption secret.</param>
		/// <param name="iv">The encryption initialization vector.</param>
		/// <returns>The cipher.</returns>
		/// <exception cref="ArgumentNullException">Thrown if either
		///		<paramref name="cipher"/>, <paramref name="key"/>,
		///		or <paramref name="iv"/> is null.</exception>
		///	<exception cref="ArgumentOutOfRangeException">Thrown if the length
		///		of either <paramref name="key"/> is not 32 or of
		///		<paramref name="iv"/> is not 16.</exception>
		public static async Task<byte[]> Encrypt(byte[] data, byte[] key, byte[] iv)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (key.Length != 32) throw new ArgumentOutOfRangeException(nameof(key));
			if (iv == null) throw new ArgumentNullException(nameof(iv));
			if (iv.Length != 16) throw new ArgumentOutOfRangeException(nameof(iv));

			byte[] cipher = null;
			if (data.Length > 0)
			{
				using (Aes aes = Aes.Create())
				{
					aes.Key = key;
					aes.IV = iv;
					ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

					using (MemoryStream memoryStream = new MemoryStream())
					{
						using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
						{
							await cryptoStream.WriteAsync(data, 0, data.Length);
							cryptoStream.FlushFinalBlock();
							cipher = memoryStream.ToArray();
						}
					}
				}
			}

			return cipher ?? new byte[0];
		}

		/// <summary>
		/// Decrypts a <paramref name="cipher"/>.
		/// </summary>
		/// <param name="cipher">The cipher.</param>
		/// <param name="key">The encryption secret.</param>
		/// <param name="iv">The encryption initialization vector.</param>
		/// <returns>The plain data.</returns>
		/// <exception cref="ArgumentNullException">Thrown if either
		///		<paramref name="cipher"/>, <paramref name="key"/>,
		///		or <paramref name="iv"/> is null.</exception>
		///	<exception cref="ArgumentOutOfRangeException">Thrown if the length
		///		of either <paramref name="key"/> is not 32 or of
		///		<paramref name="iv"/> is not 16.</exception>
		public static async Task<byte[]> Decrypt(byte[] cipher, byte[] key, byte[] iv)
		{
			if (cipher == null) throw new ArgumentNullException(nameof(cipher));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (key.Length != 32) throw new ArgumentOutOfRangeException(nameof(key));
			if (iv == null) throw new ArgumentNullException(nameof(iv));
			if (iv.Length != 16) throw new ArgumentOutOfRangeException(nameof(iv));

			byte[] data = null;
			if (cipher.Length > 0)
			{
				using (Aes aes = Aes.Create())
				{
					aes.Key = key;
					aes.IV = iv;
					ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

					using (MemoryStream memoryStream = new MemoryStream(cipher))
					{
						using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
						{
							using (MemoryStream outputStream = new MemoryStream())
							{
								int? roundLength = null;
								var roundBuffer = new byte[1024];
								while ((roundLength == null) || (roundLength == roundBuffer.Length))
								{
									roundLength = await cryptoStream.ReadAsync(roundBuffer, 0, roundBuffer.Length);
									if (roundLength > 0)
									{
										await outputStream.WriteAsync(roundBuffer, 0, roundLength.Value);
									}
								}

								data = outputStream.ToArray();
							}
						}
					}
				}
			}

			return data;
		}
	}
}