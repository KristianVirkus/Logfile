using NUnit.Framework;
using FluentAssertions;
using Logfile.Core.Details;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logfile.Core.UnitTests.Details
{
	class EventIDTest
	{
		[ID(1)]
		class EventClass1
		{
			[ID(2)]
			public class EventClass2
			{
				[ID(3)]
				public enum EventEnum
				{
					Event1 = 1,
					[Parameters("firstName", "lastName", "yearOfBirth")]
					Event2 = 2,
					Event3 = 3,
				}
			}
		}

		[ID(1)]
		struct EventStruct1
		{
			[ID(2)]
			public class EventStruct2
			{
				[ID(3)]
				public enum EventEnum
				{
					Event1 = 1,
					Event2 = 2,
					Event3 = 3,
				}
			}
		}

		[ID(1)]
		public enum OtherTypeEnum : long
		{
			Event1 = 1,
			Event2 = 2,
			Event3 = 3,
		}

		[ID(1)]
		class EventClassSkipped
		{
			public class EventClass2
			{
				[ID(3)]
				public enum EventEnum
				{
					Event1 = 1,
					Event2 = 2,
					Event3 = 3,
				}
			}
		}

		[ID(1)]
		class EventEnumWithoutSubID
		{
			public enum EventEnum
			{
				Event1 = 1,
				Event2 = 2,
				Event3 = 3,
			}
		}

		class EventEnumWithoutAnyID
		{
			public enum EventEnum
			{
				Event1 = 1,
				Event2 = 2,
				Event3 = 3,
			}
		}

		class UnstringifyableType
		{
			public override string ToString()
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void TextChainNull_Should_SetProperties()
		{
			var eventID = new EventID(null, new[] { 1, 2, 3 });
			eventID.ToString().Should().Be("1.2.3");
		}

		[Test]
		public void NumberChainNull_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, null);
			eventID.ToString().Should().Be("A.B.C");
		}

		[Test]
		public void Constructor_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, new[] { 1, 2, 3 });
			eventID.ToString().Should().Be("A.B.C (1.2.3)");
		}

		[Test]
		public void LogEventNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => EventIDExtensions.Event<StandardLoglevel, EventClass1.EventClass2.EventEnum>(null, EventClass1.EventClass2.EventEnum.Event2));
		}

		[Test]
		public void EnumInClass_Should_ListAllLevels()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventClass1.EventClass2.EventEnum.Event1);
			var eventID = logEvent.Details.OfType<EventID<EventClass1.EventClass2.EventEnum>>().Single();
			eventID.Enum.Should().Be(EventClass1.EventClass2.EventEnum.Event1);
			eventID.ToString().Should().Be("EventClass1.EventClass2.EventEnum.Event1 (1.2.3.1)");
		}

		[Test]
		public void EnumInStruct_Should_ListAllLevels()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventStruct1.EventStruct2.EventEnum.Event1);
			var eventID = logEvent.Details.OfType<EventID<EventStruct1.EventStruct2.EventEnum>>().Single();
			eventID.Enum.Should().Be(EventStruct1.EventStruct2.EventEnum.Event1);
			eventID.ToString().Should().Be("EventStruct1.EventStruct2.EventEnum.Event1 (1.2.3.1)");
		}

		[Test]
		public void EnumInSkippedClass_Should_ListAllLevelsExceptSkipped()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventClassSkipped.EventClass2.EventEnum.Event1);
			var eventID = logEvent.Details.OfType<EventID<EventClassSkipped.EventClass2.EventEnum>>().Single();
			eventID.Enum.Should().Be(EventClassSkipped.EventClass2.EventEnum.Event1);
			eventID.ToString().Should().Be("EventClassSkipped.EventEnum.Event1 (1.3.1)");
		}

		[Test]
		public void EventWithTooFewArguments_Should_ReportMissingArgumentsAsNull()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventClass1.EventClass2.EventEnum.Event2, "Jon", "Doe");
			var eventID = logEvent.Details.OfType<EventID<EventClass1.EventClass2.EventEnum>>().Single();
			eventID.ParameterNames.Should().Contain("firstName", "lastName", "yearOfBirth");
			eventID.StringArguments.Should().Contain("Jon", "Doe");
		}

		[Test]
		public void EventWithTooManyArguments_Should_KeepArguments()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventClass1.EventClass2.EventEnum.Event2, "Jon", "Doe", 1980, 1, 2, 3);
			var eventID = logEvent.Details.OfType<EventID<EventClass1.EventClass2.EventEnum>>().Single();
			eventID.ParameterNames.Should().Contain("firstName", "lastName", "yearOfBirth");
			eventID.StringArguments.Should().Contain("Jon", "Doe", "1980", "1", "2", "3");
		}

		[Test]
		public void EventWithSingleNullArgument_Should_NotHaveAnyArguments()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventClass1.EventClass2.EventEnum.Event2, (object[])null);
			var eventID = logEvent.Details.OfType<EventID<EventClass1.EventClass2.EventEnum>>().Single();
			eventID.ParameterNames.Should().Contain("firstName", "lastName", "yearOfBirth");
			eventID.StringArguments.Should().BeNull();
		}

		[Test]
		public void EventWithNullArguments_Should_KeepArguments()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventClass1.EventClass2.EventEnum.Event2, null, null, null);
			var eventID = logEvent.Details.OfType<EventID<EventClass1.EventClass2.EventEnum>>().Single();
			eventID.ParameterNames.Should().Contain("firstName", "lastName", "yearOfBirth");
			eventID.StringArguments.Count().Should().Be(3);
			eventID.StringArguments.ElementAt(0).Should().BeNull();
			eventID.StringArguments.ElementAt(1).Should().BeNull();
			eventID.StringArguments.ElementAt(2).Should().BeNull();
		}

		[Test]
		public void EnumWithParametersAndNonStringifyableArgument_Should_UseArgumentTypeNameInstead()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventClass1.EventClass2.EventEnum.Event2, "Jon", "Doe", new UnstringifyableType());
			var eventID = logEvent.Details.OfType<EventID<EventClass1.EventClass2.EventEnum>>().Single();
			eventID.StringArguments.Last().Should().Be(typeof(UnstringifyableType).FullName);
		}

		[Test]
		public void EnumWithOtherType_Should_NotAddEventIDToLogEvent()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, OtherTypeEnum.Event1);
			var eventID = logEvent.Details.OfType<EventID<EventClass1.EventClass2.EventEnum>>().Any().Should().BeFalse();
		}

		[Test]
		public void EnumInClass_Should_ListAllLevelsExceptEnumItself()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventEnumWithoutSubID.EventEnum.Event1);
			var eventID = logEvent.Details.OfType<EventID<EventEnumWithoutSubID.EventEnum>>().Single();
			eventID.Enum.Should().Be(EventEnumWithoutSubID.EventEnum.Event1);
			eventID.ToString().Should().Be("EventEnumWithoutSubID.Event1 (1.1)");
		}

		[Test]
		public void EnumInClassWithoutAnyIDs_Should_UseEnumNumberOnly()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			EventIDExtensions.Event(logEvent, EventEnumWithoutAnyID.EventEnum.Event1);
			var eventID = logEvent.Details.OfType<EventID<EventEnumWithoutAnyID.EventEnum>>().Single();
			eventID.Enum.Should().Be(EventEnumWithoutAnyID.EventEnum.Event1);
			eventID.ToString().Should().Be("Event1 (1)");
		}
	}
}
