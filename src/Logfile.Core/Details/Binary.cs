using System;
using System.Collections.Generic;
using System.Text;

namespace Logfile.Core.Details
{
	/// <summary>
	/// Represents binary data log event details.
	/// </summary>
	public class Binary
	{
		/// <summary>
		/// Gets the data.
		/// </summary>
		public IReadOnlyList<byte> Data { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Binary"/> class.
		/// </summary>
		/// <param name="data">The data.</param>
		public Binary(IReadOnlyList<byte> data)
		{
			this.Data = data;
		}

		public override string ToString()
		{
			if (this.Data == null) return "null";
			return string.Format("{0} Byte{1}", this.Data.Count, (this.Data.Count == 1 ? "" : "s"));
		}
	}

	/// <summary>
	/// Implements extension methods for fluent event creation.
	/// </summary>
	public static class BinaryExtensions
	{
		/// <summary>
		/// Adds binary data to a log event.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="logEvent">The log event.</param>
		/// <param name="data">The binary data.</param>
		/// <returns>The same <paramref name="logEvent"/> instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logEvent"/> is null.</exception>
		public static LogEvent<TLoglevel> Binary<TLoglevel>(this LogEvent<TLoglevel> logEvent, IReadOnlyList<byte> data)
			where TLoglevel : Enum
		{
			if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
			if (data != null) logEvent.Details.Add(new Binary(data));
			return logEvent;
		}
	}
}
