﻿using System.Collections.Generic;

namespace Logfile.Core.Details
{
	/// <summary>
	/// Represents the logfile hierarchy log event details.
	/// </summary>
	public class LogfileHierarchy
	{
		/// <summary>
		/// Gets the logfile hierarchy generated by cloning.
		/// </summary>
		public IEnumerable<string> Hierarchy { get; } = new string[0];

		/// <summary>
		/// Initializes a new instance of the <see cref="LogfileHierarchy"/> class.
		/// </summary>
		/// <param name="hierarchy">The hierarchy of logfile names, possibly proxy names.</param>
		public LogfileHierarchy(IEnumerable<string> hierarchy)
		{
			if (hierarchy != null) this.Hierarchy = hierarchy;
		}

		public override string ToString()
		{
			return string.Join(".", this.Hierarchy ?? new string[0]);
		}
	}
}