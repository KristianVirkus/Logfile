namespace Logfile.Core
{
	/// <summary>
	/// Enumeration of standard loglevels.
	/// </summary>
	public enum StandardLoglevel
	{
		/// <summary>
		/// Events for tracing of internal operations.
		/// </summary>
		Trace = 0,

		/// <summary>
		/// Debugging events.
		/// </summary>
		Debug = 1,

		/// <summary>
		/// Informational events.
		/// </summary>
		Information = 2,

		/// <summary>
		/// Warning events.
		/// </summary>
		Warning = 3,

		/// <summary>
		/// Error events.
		/// </summary>
		Error = 4,

		/// <summary>
		/// Critial events.
		/// </summary>
		Critical = 5,
	}
}
