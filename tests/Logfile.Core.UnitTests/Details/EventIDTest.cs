﻿using FluentAssertions;
using Logfile.Core.Details;
using NUnit.Framework;
using System;
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
		public void ConstructorWithoutArguments_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, new[] { 1, 2, 3 });
			eventID.ToString().Should().Be("A.B.C (1.2.3)");
		}

		[Test]
		public void ConstructorWithArguments_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, new[] { 1, 2, 3 }, "arg1", "arg2", "arg3");
			eventID.ToString().Should().Be(@"A.B.C (1.2.3) {""arg1"",""arg2"",""arg3""}");
		}

		[Test]
		public void ConstructorWithParameterNamesNullAndArgumentsNull_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, new[] { 1, 2, 3 }, parameterNames: null, args: null);
			eventID.ToString().Should().Be(@"A.B.C (1.2.3)");
		}

		[Test]
		public void ConstructorWithParameterNamesNullAndArgumentsEmpty_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, new[] { 1, 2, 3 }, parameterNames: null, args: new string[0]);
			eventID.ToString().Should().Be(@"A.B.C (1.2.3)");
		}

		[Test]
		public void ConstructorWithParameterNamesAndArgumentsNull_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, new[] { 1, 2, 3 }, parameterNames: new[] { "p1", "p2" }, args: new string[0]);
			eventID.ToString().Should().Be(@"A.B.C (1.2.3)");
		}

		[Test]
		public void ConstructorWithReducedParameterNamesAndMoreArguments_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, new[] { 1, 2, 3 }, parameterNames: new[] { "p1", "p2" }, args: new[] { "arg1", "arg2", "arg3" });
			eventID.ToString().Should().Be(@"A.B.C (1.2.3) {p1=""arg1"",p2=""arg2"",""arg3""}");
		}

		[Test]
		public void ConstructorWithParameterNamesAndArgumentsIncludingNull_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, new[] { 1, 2, 3 }, parameterNames: new[] { "p1", "p2" }, args: new[] { null, "arg2" });
			eventID.ToString().Should().Be(@"A.B.C (1.2.3) {p1=null,p2=""arg2""}");
		}

		[Test]
		public void ConstructorWithParameterNamesIncludingNullAndArguments_Should_SetProperties()
		{
			var eventID = new EventID(new[] { "A", "B", "C" }, new[] { 1, 2, 3 }, parameterNames: new[] { null, "p2" }, args: new[] { "arg1", "arg2" });
			eventID.ToString().Should().Be(@"A.B.C (1.2.3) {""arg1"",p2=""arg2""}");
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

		[Test]
		public void ConstructorWithProductIDNullAndNoParameterNames_Should_SetProperties()
		{
			// Arrange
			// Act
			var obj = new EventID(
				productID: null,
				textChain: new[] { "a", "b" },
				numberChain: new[] { 1, 2 },
				args: new[] { "arg1", "arg2" });

			// Assert
			obj.ProductID.Should().BeNull();
			obj.TextChain.Should().Contain("a", "b");
			obj.NumberChain.Should().Contain(new[] { 1, 2 });
			obj.StringArguments.Should().Contain("arg1", "arg2");
		}

		[Test]
		public void ConstructorWithProductIDAndNoParameterNames_Should_SetProperties()
		{
			// Arrange
			// Act
			var obj = new EventID(
				productID: "my-product",
				textChain: new[] { "a", "b" },
				numberChain: new[] { 1, 2 },
				args: new[] { "arg1", "arg2" });

			// Assert
			obj.ProductID.Should().Be("my-product");
			obj.TextChain.Should().Contain("a", "b");
			obj.NumberChain.Should().Contain(new[] { 1, 2 });
			obj.StringArguments.Should().Contain("arg1", "arg2");
		}

		[Test]
		public void ConstructorWithProductIDNullAndWithParameterNames_Should_SetProperties()
		{
			// Arrange
			// Act
			var obj = new EventID(
				productID: null,
				textChain: new[] { "a", "b" },
				numberChain: new[] { 1, 2 },
				parameterNames: new[] { "p1", "p2" },
				args: new[] { "arg1", "arg2" });

			// Assert
			obj.ProductID.Should().BeNull();
			obj.TextChain.Should().Contain("a", "b");
			obj.NumberChain.Should().Contain(new[] { 1, 2 });
			obj.ParameterNames.Should().Contain("p1", "p2");
			obj.StringArguments.Should().Contain("arg1", "arg2");
		}

		[Test]
		public void ConstructorWithProductIDAndWithParameterNames_Should_SetProperties()
		{
			// Arrange
			// Act
			var obj = new EventID(
				productID: "my-product",
				textChain: new[] { "a", "b" },
				numberChain: new[] { 1, 2 },
				parameterNames: new[] { "p1", "p2" },
				args: new[] { "arg1", "arg2" });

			// Assert
			obj.ProductID.Should().Be("my-product");
			obj.TextChain.Should().Contain("a", "b");
			obj.NumberChain.Should().Contain(new[] { 1, 2 });
			obj.ParameterNames.Should().Contain("p1", "p2");
			obj.StringArguments.Should().Contain("arg1", "arg2");
		}

		[ProductID("pitl")]
		private static class TestProductIDTopLevel
		{
			[ID(1)]
			public static class Nested1
			{
				[ID(2)]
				public enum Events
				{
					Event = 7,
				}
			}
		}

		[Test]
		public void ProductIDInSeparateTopLevelClass_Should_RetainProductIDInNestedItems()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			// Act
			logEvent.Event(TestProductIDTopLevel.Nested1.Events.Event);

			// Assert
			logEvent.Details.OfType<EventID>().Single().ProductID.Should().Be("pitl");
		}

		[ProductID("top-level")]
		private static class TestProductIDOverwrittenIn2ndNestedClass
		{
			[ID(1)]
			public static class Nested1
			{
				[ProductID("nested2")]
				[ID(2)]
				public static class Nested2
				{
					[ID(3)]
					public enum Events
					{
						Event = 7,
					}
				}
			}
		}

		[Test]
		public void ProductIDOverwrittenIn2ndNestedClass_Should_ReflectNestedProductID()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			// Act
			logEvent.Event(TestProductIDOverwrittenIn2ndNestedClass.Nested1.Nested2.Events.Event);

			// Assert
			logEvent.Details.OfType<EventID>().Single().ProductID.Should().Be("nested2");
		}

		[ProductID("top-level")]
		private static class TestProductIDOverwrittenWithNullInNestedClass
		{
			[ProductID(null)]
			[ID(1)]
			public static class Nested1
			{
				[ID(2)]
				public enum Events
				{
					Event = 7,
				}
			}
		}

		[Test]
		public void ProductIDOverwrittenWithNullInNestedClass_Should_KeepTopLevelProductID()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			// Act
			logEvent.Event(TestProductIDOverwrittenWithNullInNestedClass.Nested1.Events.Event);

			// Assert
			logEvent.Details.OfType<EventID>().Single().ProductID.Should().Be("top-level");
		}

		[ProductID("top-level")]
		private static class TestProductIDOverwrittenWithEmptyProductIDInNestedClass
		{
			[ProductID("")]
			[ID(1)]
			public static class Nested1
			{
				[ID(2)]
				public enum Events
				{
					Event = 7,
				}
			}
		}

		[Test]
		public void ProductIDOverwrittenWithEmptyProductIDInNestedClass_Should_RemoveProductID()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			// Act
			logEvent.Event(TestProductIDOverwrittenWithEmptyProductIDInNestedClass.Nested1.Events.Event);

			// Assert
			logEvent.Details.OfType<EventID>().Single().ProductID.Should().BeNull();
		}

		[ProductID("top-level")]
		[ID(1)]
		private static class TestProductIDTogetherWithEventIDClass
		{
			[ID(2)]
			public enum Events
			{
				Event = 7,
			}
		}

		[Test]
		public void ProductIDTogetherWithEventIDOnClass_Should_UseBothIDs()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			// Act
			logEvent.Event(TestProductIDTogetherWithEventIDClass.Events.Event);

			// Assert
			logEvent.Details.OfType<EventID>().Single().ProductID.Should().Be("top-level");
			logEvent.Details.OfType<EventID>().Single().NumberChain.Should().Contain(new[] { 1, 2, 7 });
		}

		[ProductID("top-level")]
		[ID(1)]
		public enum TestProductIDTogetherWithEventIDEnum
		{
			Event = 7,
		}

		[Test]
		public void ProductIDTogetherWithEventIDOnEnum_Should_UseBothIDs()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			// Act
			logEvent.Event(TestProductIDTogetherWithEventIDEnum.Event);

			// Assert
			logEvent.Details.OfType<EventID>().Single().ProductID.Should().Be("top-level");
			logEvent.Details.OfType<EventID>().Single().NumberChain.Should().Contain(new[] { 1, 7 });
		}

		[Test]
		public void OverriddenProductID_Should_BeOutputByToString()
		{
			// Arrange
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			// Act
			logEvent.Event(TestProductIDOverwrittenIn2ndNestedClass.Nested1.Nested2.Events.Event, "arg1", "arg2");

			// Assert
			logEvent.Details.OfType<EventID>().Single().ToString().Should().Be(@"nested2/Nested1.Nested2.Events.Event (nested2/1.2.3.7) {""arg1"",""arg2""}");
		}

		[Test]
		public void ProductIDConstructorWithIDNull_Should_SetProperties()
		{
			// Arrange
			// Act
			var obj = new ProductIDAttribute(id: null);

			// Assert
			obj.ID.Should().BeNull();
		}

		[Test]
		public void ProductIDConstructorWithID_Should_SetProperties()
		{
			// Arrange
			// Act
			var obj = new ProductIDAttribute(id: "test");

			// Assert
			obj.ID.Should().Be("test");
		}

		[Test]
		public void EventIDConstructor_Should_SetProperties()
		{
			// Arrange
			// Act
			var obj = new IDAttribute(id: 1337);

			// Assert
			obj.ID.Should().Be(1337);
		}

		[Test]
		public void ParametersConstructorWithParameterNamesNull_Should_SetProperties()
		{
			// Arrange
			// Act
			var obj = new ParametersAttribute(parameterNames: null);

			// Assert
			obj.ParameterNames.Should().BeNull();
		}

		[Test]
		public void ParametersConstructorWithParameterNames_Should_SetProperties()
		{
			// Arrange
			// Act
			var obj = new ParametersAttribute("p1", "p2");

			// Assert
			obj.ParameterNames.Should().Contain("p1", "p2");
		}
	}
}
