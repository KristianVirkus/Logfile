using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Logfile.Core.Details
{
	/// <summary>
	/// Represents event ID log event details.
	/// </summary>
	public class EventID
	{
		/// <summary>
		/// Gets the chain of texts representing the event.
		/// </summary>
		public IEnumerable<string> TextChain { get; protected set; }

		/// <summary>
		/// Gets the chain of numbers representing the event.
		/// </summary>
		public IEnumerable<int> NumberChain { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EventID"/> class.
		/// </summary>
		/// <param name="textChain">The chain of texts representing
		///		the event.</param>
		/// <param name="numberChain">The chain of numbers representing
		///		the event.</param>
		public EventID(IEnumerable<string> textChain, IEnumerable<int> numberChain)
		{
			this.TextChain = textChain;
			this.NumberChain = numberChain;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append(string.Join(".", this.TextChain ?? new string[0]));

			var numbers = string.Join(".", this.NumberChain ?? new int[0]);
			if (numbers.Length > 0)
			{
				if (sb.Length > 0) numbers = $" ({numbers})";
				sb.Append(numbers);
			}

			return sb.ToString();
		}
	}

	/// <summary>
	/// Represents an event ID.
	/// </summary>
	/// <typeparam name="TEvent">The log event type.</typeparam>
	public class EventID<TEvent> : EventID
		where TEvent : Enum
	{
		static readonly ReaderWriterLockSlim sync;
		static readonly Dictionary<Type, CachedItem> typeCache;
		static readonly Dictionary<TEvent, CachedEvent> eventCache;

		/// <summary>
		/// The event enumeration member.
		/// </summary>
		public TEvent Enum { get; }

		/// <summary>
		/// Gets the event parameter names.
		/// </summary>
		public IEnumerable<string> ParameterNames { get; }

		/// <summary>
		/// The event arguments.
		/// </summary>
		public IEnumerable<string> StringArguments { get; }

		static EventID()
		{
			sync = new ReaderWriterLockSlim();
			typeCache = new Dictionary<Type, CachedItem>();
			eventCache = new Dictionary<TEvent, CachedEvent>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventID"/> class.
		/// </summary>
		/// <param name="e">The event enum member.</param>
		/// <param name="args">The event arguments.</param>
		public EventID(TEvent e, params string[] args)
			: base(null, null)
		{
			this.Enum = e;
			this.StringArguments = args;

			sync.EnterUpgradeableReadLock();
			try
			{
				if (!eventCache.TryGetValue(e, out var cachedEvent))
				{
					// Cache event.
					sync.EnterWriteLock();
					try
					{
						var type = getTypeOrCacheType(e.GetType());

						IEnumerable<string> parameterNames = null;
						var parametersAtts = from d in e.GetType().GetMember(e.ToString()).Single().GetCustomAttributesData()
											 where d.AttributeType.IsAssignableFrom(typeof(ParametersAttribute))
											 select d;
						var parametersAtt = parametersAtts.LastOrDefault();
						if (parametersAtt != null)
						{
							parameterNames = (parametersAtt.ConstructorArguments.OfType<CustomAttributeTypedArgument?>().FirstOrDefault()?.Value as ReadOnlyCollection<CustomAttributeTypedArgument>)?.Select(a => a.Value?.ToString());
						}

						cachedEvent = new CachedEvent(
							e.GetType(),
							true,
							e.ToString(),
							Convert.ToInt32(e),
							parameterNames,
							type?.TextChain?.Concat(new[] { e.ToString() }) ?? new[] { e.ToString() },
							type?.NumberChain?.Concat(new[] { Convert.ToInt32(e) }) ?? new[] { Convert.ToInt32(e) });

						eventCache.Add(e, cachedEvent);
					}
					finally
					{
						sync.ExitWriteLock();
					}
				}

				this.TextChain = cachedEvent.TextChain;
				this.NumberChain = cachedEvent.NumberChain;
				this.ParameterNames = cachedEvent.ParameterNames;
			}
			finally
			{
				sync.ExitUpgradeableReadLock();
			}
		}

		/// <summary>
		/// Gets an event's <paramref name="type"/> if already cached or determines all
		/// information and creates the cache item. Must be executed in a write-synced
		/// environment.
		/// </summary>
		/// <param name="type">The type to find or cache.</param>
		/// <returns>The cached type information.</returns>
		static CachedItem getTypeOrCacheType(Type type)
		{
			// Handle type (or declaring type from recursion) is object or null.
			if ((type == null) || (type == typeof(object))) return null;

			// If this type is already cached, return cached value.
			if (typeCache.TryGetValue(type, out var cachedItem)) return cachedItem;

			// Get cached base type or have it created.
			CachedItem cachedParentItem = getTypeOrCacheType(type.DeclaringType);
			var textChain = (cachedParentItem?.TextChain ?? new string[0]);
			var numberChain = (cachedParentItem?.NumberChain ?? new int[0]);

			// Get and cache information of this type.
			var number = type.GetCustomAttributesData()
							.Where(a => a.AttributeType == typeof(IDAttribute))
							.LastOrDefault()
							?.ConstructorArguments
								.Single()
								.Value as int?;
			if (number != null)
			{
				textChain = textChain.Concat(new[] { type.Name });
				numberChain = numberChain.Concat(new[] { (int)number });

				// Create new cached item.
				cachedItem = new CachedItem(type, (number != null), type.Name, 0, textChain, numberChain);
				typeCache.Add(type, cachedItem);
				return cachedItem;
			}
			else
			{
				return cachedParentItem;
			}
		}
	}

	/// <summary>
	/// Represents an event ID attribute to put before any class, struct,
	/// or enum definition to be given a designated number in a chained event ID.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
	public class IDAttribute : Attribute
	{
		/// <summary>
		/// Gets the event ID in this level.
		/// </summary>
		public int ID { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="IDAttribute"/> class.
		/// </summary>
		/// <param name="id">The event ID.</param>
		public IDAttribute(int id)
		{
			this.ID = id;
		}
	}

	/// <summary>
	/// Represents an event parameters attribute to put before any event enum member
	/// to be assigned named parameters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ParametersAttribute : Attribute
	{
		/// <summary>
		/// Gets the parameter names.
		/// </summary>
		public IEnumerable<string> ParameterNames { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ParametersAttribute"/> class.
		/// </summary>
		/// <param name="parameterNames">The parameter names.</param>
		public ParametersAttribute(params string[] parameterNames)
		{
			this.ParameterNames = parameterNames;
		}
	}

	class CachedItem
	{
		public Type Type { get; }
		public bool IsIncluded { get; }
		public string Name { get; }
		public int Number { get; }
		public IEnumerable<string> TextChain { get; }
		public IEnumerable<int> NumberChain { get; }

		public CachedItem(Type type, bool isIncluded, string name, int number,
			IEnumerable<string> textChain, IEnumerable<int> numberChain)
		{
			this.Type = type;
			this.IsIncluded = isIncluded;
			this.Name = name;
			this.Number = number;
			this.TextChain = textChain;
			this.NumberChain = numberChain;
		}
	}

	class CachedEvent : CachedItem
	{
		public IEnumerable<string> ParameterNames { get; }

		public CachedEvent(Type type, bool isIncluded, string name, int number,
			IEnumerable<string> parameterNames, IEnumerable<string> textChain,
			IEnumerable<int> numberChain)
			: base(type, isIncluded, name, number, textChain, numberChain)
		{
			this.ParameterNames = parameterNames;
		}
	}

	/// <summary>
	/// Implements extension methods for fluent event creation.
	/// </summary>
	public static class EventIDExtensions
	{
		/// <summary>
		/// Adds an event ID <paramref name="e"/> to a <paramref name="logEvent"/>.
		/// </summary>
		/// <typeparam name="TLoglevel">The loglevel type.</typeparam>
		/// <typeparam name="TEvent">The event type.</typeparam>
		/// <param name="logEvent">The log event.</param>
		/// <param name="e">The event.</param>
		/// <param name="args">Arguments for the event <paramref name="e"/>.</param>
		/// <returns><paramref name="logEvent"/></returns>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logEvent"/> is null.</exception>
		public static LogEvent<TLoglevel> Event<TLoglevel, TEvent>(this LogEvent<TLoglevel> logEvent,
			TEvent e, params object[] args)
			where TLoglevel : Enum
			where TEvent : Enum
		{
			if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

			if (e == null) return logEvent;

			try
			{
				var eventID = new EventID<TEvent>(e, args?.Select(a =>
				{
					try
					{
						return a.ToString();
					}
					catch
					{
						return a?.GetType().FullName;
					}
				}).ToArray());
				logEvent.Details.Add(eventID);
			}
			catch
			{
			}

			return logEvent;
		}
	}
}
