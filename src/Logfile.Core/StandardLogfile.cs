namespace Logfile.Core
{
	/// <summary>
	/// Implements a logfile with standard loglevels.
	/// </summary>
	public class StandardLogfile : Logfile<StandardLoglevel>
	{
		/// <summary>
		/// Gets a new trace log event.
		/// </summary>
		public LogEvent<StandardLoglevel> Trace => this.New(StandardLoglevel.Trace);

		/// <summary>
		/// Gets a new debug log event.
		/// </summary>
		public LogEvent<StandardLoglevel> Debug => this.New(StandardLoglevel.Debug);

		/// <summary>
		/// Gets a new information log event.
		/// </summary>
		public LogEvent<StandardLoglevel> Info => this.New(StandardLoglevel.Information);

		/// <summary>
		/// Gets a new warning log event.
		/// </summary>
		public LogEvent<StandardLoglevel> Warning => this.New(StandardLoglevel.Warning);

		/// <summary>
		/// Gets a new error log event.
		/// </summary>
		public LogEvent<StandardLoglevel> Error => this.New(StandardLoglevel.Error);

		/// <summary>
		/// Gets a new critical log event.
		/// </summary>
		public LogEvent<StandardLoglevel> Critical => this.New(StandardLoglevel.Critical);
	}
}
