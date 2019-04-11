using System;
using System.Collections.Generic;
using System.Linq;

namespace Logfile.Core.Details
{
	/// <summary>
	/// Represents message log event details.
	/// </summary>
	public class Message
	{
		/// <summary>
		/// Gets the text.
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// Gets the original arguments as strings.
		/// </summary>
		public IEnumerable<string> StringArguments { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Message"/> class.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="args">The arguments as strings.</param>
		public Message(string text, IEnumerable<string> args)
		{
			this.Text = text;
			this.StringArguments = args;
		}

		public override string ToString()
		{
			if (this.StringArguments?.Any() != true) return this.Text;

			try
			{

				return string.Format(this.Text, this.StringArguments.ToArray<object>());
			}
			catch
			{
				var argsString = string.Join(", ", this.StringArguments.Select(a => $"\"{a ?? "null"}\""));
				return $"{this.Text} {{{argsString}}}";
			}
		}
	}

	/// <summary>
	/// Implements extension methods for fluent event creation.
	/// </summary>
	public static class MessageExtensions
	{
		/// <summary>
		/// Adds a message <paramref name="text"/> to a <paramref name="logEvent"/>.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <param name="logEvent">The log event.</param>
		/// <param name="text">The text. Can be a format string. If null, no text will be added.</param>
		/// <param name="args">Arguments for formatting the <paramref name="text"/>.</param>
		/// <returns><paramref name="logEvent"/></returns>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logEvent"/> is null.</exception>
		public static LogEvent<TLoglevel> Msg<TLoglevel>(this LogEvent<TLoglevel> logEvent, string text, params object[] args)
			where TLoglevel : Enum
		{
			if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

			try
			{
				var stringArgs = args?.Select(a =>
				{
					try
					{
						return a?.ToString();
					}
					catch
					{
						return a.GetType().Name;
					}
				});

				if (text != null)
					logEvent.Details.Add(new Message(text, stringArgs));
			}
			catch
			{
			}

			return logEvent;
		}
	}
}
