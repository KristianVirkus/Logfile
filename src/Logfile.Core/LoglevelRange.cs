using System;

namespace Logfile.Core
{
	/// <summary>
	/// Represents a range of loglevels.
	/// </summary>
	/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
	class LoglevelRange<TLoglevel>
		where TLoglevel : Enum
	{
		/// <summary>
		/// Gets the loglevel to start with, less severe.
		/// </summary>
		public TLoglevel From { get; }

		/// <summary>
		/// Gets the loglevel to end with, more severe.
		/// </summary>
		public TLoglevel To { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoglevelRange{TLoglevel}"/> class.
		/// </summary>
		/// <param name="from">The loglevel to start with. The order is irrelevant.</param>
		/// <param name="to">The loglevel to end with. The order is irrelevant.</param>
		public LoglevelRange(TLoglevel from, TLoglevel to)
		{
			if (from.CompareTo(to) == 1)
			{
				var temp = to;
				to = from;
				from = temp;
			}

			this.From = from;
			this.To = to;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoglevelRange{TLoglevel}"/> class.
		/// </summary>
		/// <param name="loglevel">A single loglevel.</param>
		public LoglevelRange(TLoglevel loglevel)
		{
			this.From = loglevel;
			this.To = loglevel;
		}

		/// <summary>
		/// Checks whether the this loglevel range covers a specific
		/// <paramref name="loglevel"/>.
		/// </summary>
		/// <param name="loglevel">The loglevel to check.</param>
		/// <returns>true if the <paramref name="loglevel"/> is within this range
		///		of loglevels, false otherwise.</returns>
		public bool CheckIsCovered(TLoglevel loglevel) => ((loglevel.CompareTo(this.From) >= 0) && (loglevel.CompareTo(this.To) <= 0));
	}
}
