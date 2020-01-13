using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Logfile.Core.Details
{
	/// <summary>
	/// Represents arbitrary log event arguments.
	/// </summary>
	public class Arguments
	{
		/// <summary>
		/// Get the arguments with their optional names.
		/// </summary>
		public IEnumerable<(string Name, object Value)> Values { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Arguments"/> class.
		/// </summary>
		/// <param name="values">The arguments with their optional names.</param>
		public Arguments(IEnumerable<(string Name, object Value)> values)
		{
			this.Values = values;
		}

		/// <summary>
		/// Outputs the arguments with their optional names as a string.
		/// </summary>
		/// <returns>The string.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("{");

			if (this.Values?.Any() == true)
			{
				var first = true;
				foreach (var arg in this.Values)
				{
					if (!first)
						sb.Append(",");
					else
						first = false;

					if (!string.IsNullOrEmpty(arg.Name))
						sb.Append($"{arg.Name}=");

					if (arg.Value == null)
						sb.Append("null");
					else
						sb.Append($@"""{arg.Value}""");
				}
			}

			sb.Append("}");
			return sb.ToString();
		}
	}

	/// <summary>
	/// Implements extension methods for arguments event details.
	/// </summary>
	public static class ArgumentsExtensions
	{
		/// <summary>
		/// Adds arbitrary <paramref name="args"/> to a <paramref name="logEvent"/>.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="logEvent">The log event to extend.</param>
		/// <param name="args">The arguments. If null, the
		///		<paramref name="logEvent"/> will not get extended.</param>
		/// <returns>The same <paramref name="logEvent"/> to chain calls.</returns>
		/// <exception cref="ArgumentNullException">Thrown, if
		///		<paramref name="logEvent"/> is null.</exception>
		public static LogEvent<TLoglevel> Args<TLoglevel>(this LogEvent<TLoglevel> logEvent,
			params object[] args)
			where TLoglevel : Enum
		=> NamedArgs(logEvent, args: args?.Select<object, (string Name, object Value)>(a => (Name: null, Value: a)).ToArray());

		/// <summary>
		/// Adds arbitrary <paramref name="args"/> with names and values
		///		to a <paramref name="logEvent"/>.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="logEvent">The log event to extend.</param>
		/// <param name="args">The arguments with names and values. The names
		///		can be null. If <paramref name="args"/> is null, the
		///		<paramref name="logEvent"/> will not get extended.</param>
		/// <returns>The same <paramref name="logEvent"/> to chain calls.</returns>
		/// <exception cref="ArgumentNullException">Thrown, if
		///		<paramref name="logEvent"/> is null.</exception>
		public static LogEvent<TLoglevel> NamedArgs<TLoglevel>(this LogEvent<TLoglevel> logEvent,
			params (string Name, object Value)[] args)
			where TLoglevel : Enum
		{
			if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

			if (args != null)
			{
				var arguments = new Arguments(args);
				logEvent.Details.Add(arguments);
			}

			return logEvent;
		}

		/// <summary>
		/// Adds an <paramref name="obj"/>'s public instance properties and
		/// values to a <paramref name="logEvent"/> as arguments.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="logEvent">The log event to extend.</param>
		/// <param name="obj">The object. If null, the
		///		<paramref name="logEvent"/> will not get extended. If any
		///		public property's getter throws an exception, the message
		///		text will be used as value.</param>
		///	<param name="stringify">true to immediately use every property's
		///		<c>ToString</c> method result, instead of retaining the original
		///		property value's reference, false to keep the every property's
		///		original value reference.</param>
		/// <returns>The same <paramref name="logEvent"/> to chain calls.</returns>
		/// <exception cref="ArgumentNullException">Thrown, if
		///		<paramref name="logEvent"/> is null.</exception>
		public static LogEvent<TLoglevel> Props<TLoglevel>(this LogEvent<TLoglevel> logEvent,
			object obj, bool stringify = false)
			where TLoglevel : Enum
		{
			if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

			if (obj != null)
			{
				var results = new List<(string Name, object Value)>();
				var type = obj.GetType();
				var props = type.GetProperties(System.Reflection.BindingFlags.Instance
												| System.Reflection.BindingFlags.Public);
				foreach (var p in props)
				{
					object value;
					try
					{
						value = p.GetValue(obj);
						if (value != null && !(value is string) && stringify)
							value = value?.ToString();
					}
					catch (TargetInvocationException tex)
					{
						value = tex.InnerException.Message;
					}
					catch (Exception ex)
					{
						value = ex.Message;
					}

					results.Add((Name: p.Name, Value: value));
				}

				NamedArgs(logEvent, args: results.ToArray());
			}

			return logEvent;
		}
	}
}
