using FluentAssertions;
using Logfile.Core.Details;
using NUnit.Framework;
using System;
using System.Linq;

namespace Logfile.Core.UnitTests.Details
{
	static class ArgumentsTest
	{
		private static LogEvent<StandardLoglevel> createEvent()
			=> new LogEvent<StandardLoglevel>(logCallback: evt => { }, loglevel: StandardLoglevel.Warning);

		public class Constructors
		{
			[Test]
			public void ArgumentsNull_Should_SetPropertiesAndOutputArgumentsAsString()
			{
				// Arrange
				// Act
				var args = new Arguments(values: null);

				// Assert
				args.Values.Should().BeNull();
				args.ToString().Should().Be("{}");
			}

			[Test]
			public void WithArguments_Should_SetPropertiesAndOutputArgumentsAsString()
			{
				// Arrange
				var values = new (string Name, object Value)[] {
					(Name: "a", Value: 1),
					(Name: "b", Value: "Berta"),
					(Name: "c", Value: new object()),
				};

				// Act
				var args = new Arguments(values: values);

				// Assert
				args.Values.Count().Should().Be(values.Count());
				for (int i = 0; i < values.Count(); i++)
				{
					var originalValue = values.ElementAt(i);
					var argValue = args.Values.ElementAt(i);
					originalValue.Name.Should().Be(argValue.Name);
					originalValue.Value.Should().Be(argValue.Value);
				}

				args.ToString().Should().Be($@"{{a=""1"",b=""Berta"",c=""System.Object""}}");
			}

			[Test]
			public void WithArgumentsContainingNullNameAndNullValue_Should_SetPropertiesAndOutputArgumentsAsString()
			{
				// Arrange
				var values = new (string Name, object Value)[] {
					(Name: "a", Value: 1),
					(Name: "b", Value: null),
					(Name: null, Value: "c"),
					(Name: null, Value: null),
					(Name: "e", Value: "Emil"),
				};

				// Act
				var args = new Arguments(values: values);

				// Assert
				args.Values.Count().Should().Be(values.Count());
				for (int i = 0; i < values.Count(); i++)
				{
					var originalValue = values.ElementAt(i);
					var argValue = args.Values.ElementAt(i);
					originalValue.Name.Should().Be(argValue.Name);
					originalValue.Value.Should().Be(argValue.Value);
				}

				args.ToString().Should().Be($@"{{a=""1"",b=null,""c"",null,e=""Emil""}}");
			}
		}

		public static class ExtensionMethods
		{
			public class ArgsWithoutNames
			{
				[Test]
				public void LogEventNull_ShouldThrow_ArgumentNullException()
				{
					// Arrange
					// Act & Assert
					Assert.Throws<ArgumentNullException>(
						() => ArgumentsExtensions.Args<StandardLoglevel>(logEvent: null, "test"));
				}

				[Test]
				public void ArgsNull_Should_NotAddArgumentsDetails()
				{
					// Arrange
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.Args(logEvent, args: null);

					// Assert
					logEvent.Details.OfType<Arguments>().Any().Should().BeFalse();
				}

				[Test]
				public void ArgsEmpty_Should_AddEmptyArgumentsDetails()
				{
					// Arrange
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.Args(logEvent, args: new object[0]);

					// Assert
					logEvent.Details.OfType<Arguments>().Single().Values.Any().Should().BeFalse();
				}

				[Test]
				public void MultipleArgumentsDetails_Should_BeKeptSeparately()
				{
					// Arrange
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.Args(logEvent, args: new object[] { "a" });
					ArgumentsExtensions.Args(logEvent, args: new object[] { "b" });

					// Assert
					logEvent.Details.OfType<Arguments>().First().Values.Single().Value.Should().Be("a");
					logEvent.Details.OfType<Arguments>().Last().Values.Single().Value.Should().Be("b");
				}
			}

			public class ArgsWithNames
			{
				[Test]
				public void LogEventNull_ShouldThrow_ArgumentNullException()
				{
					// Arrange
					// Act & Assert
					Assert.Throws<ArgumentNullException>(
						() => ArgumentsExtensions.NamedArgs<StandardLoglevel>(
							logEvent: null,
							new (string Name, object Value)[] { (Name: "name", Value: "test") }));
				}

				[Test]
				public void ArgsNull_Should_NotAddArgumentsDetails()
				{
					// Arrange
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.NamedArgs(logEvent, args: null);

					// Assert
					logEvent.Details.OfType<Arguments>().Any().Should().BeFalse();
				}

				[Test]
				public void ArgsEmpty_Should_AddEmptyArgumentsDetails()
				{
					// Arrange
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.NamedArgs(logEvent, args: new (string Name, object Value)[0]);

					// Assert
					logEvent.Details.OfType<Arguments>().Single().Values.Any().Should().BeFalse();
				}

				[Test]
				public void MultipleArgumentsDetails_Should_BeKeptSeparately()
				{
					// Arrange
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.NamedArgs(logEvent, args: new (string Name, object Value)[] { (Name: "1", Value: "a") });
					ArgumentsExtensions.NamedArgs(logEvent, args: new (string Name, object Value)[] { (Name: "2", Value: "b") });

					// Assert
					logEvent.Details.OfType<Arguments>().First().Values.Single().Name.Should().Be("1");
					logEvent.Details.OfType<Arguments>().First().Values.Single().Value.Should().Be("a");
					logEvent.Details.OfType<Arguments>().Last().Values.Single().Name.Should().Be("2");
					logEvent.Details.OfType<Arguments>().Last().Values.Single().Value.Should().Be("b");
				}
			}

			public class Properties
			{
				private class TestClass
				{
					public string Text { get; set; }
					public int Number { get; set; }
					private bool flag { get; set; } = true;
				}

				private class TestGetterExceptionClass
				{
					public string Text { get => throw new Exception("message"); }
				}

				private class TestToStringExceptionClass
				{
					public TestProperty Property { get; set; } = new TestProperty();
				}

				private class TestProperty
				{
					public override string ToString() => throw new Exception("message");
				}

				[Test]
				public void LogEventNull_ShouldThrow_ArgumentNullException()
				{
					// Arrange
					// Act & Assert
					Assert.Throws<ArgumentNullException>(
						() => ArgumentsExtensions.Props<StandardLoglevel>(logEvent: null, obj: null));
				}

				[Test]
				public void ObjectWithNoProperties_Should_AddEmptyArgumentsDetails()
				{
					// Arrange
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.Props(logEvent, obj: new { });

					// Assert
					logEvent.Details.OfType<Arguments>().Single().Values.Any().Should().BeFalse();
				}

				[Test]
				public void ObjectWithPrivateProperties_Should_IgnorePrivateProperties()
				{
					// Arrange
					var logEvent = createEvent();
					var obj = new TestClass { Text = "test", Number = 1337 };

					// Act
					ArgumentsExtensions.Props(logEvent, obj);

					// Assert
					logEvent.Details.OfType<Arguments>().Single().Values.Count().Should().Be(2);
					logEvent.Details.OfType<Arguments>().Single().Values.Should().Contain((Name: nameof(TestClass.Text), obj.Text));
					logEvent.Details.OfType<Arguments>().Single().Values.Should().Contain((Name: nameof(TestClass.Number), obj.Number));
				}

				[Test]
				public void MultipleArgumentsDetails_Should_BeKeptSeparately()
				{
					// Arrange
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.NamedArgs(logEvent, args: new (string Name, object Value)[] { (Name: "1", Value: "a") });
					ArgumentsExtensions.NamedArgs(logEvent, args: new (string Name, object Value)[] { (Name: "2", Value: "b") });

					// Assert
					logEvent.Details.OfType<Arguments>().First().Values.Single().Name.Should().Be("1");
					logEvent.Details.OfType<Arguments>().First().Values.Single().Value.Should().Be("a");
					logEvent.Details.OfType<Arguments>().Last().Values.Single().Name.Should().Be("2");
					logEvent.Details.OfType<Arguments>().Last().Values.Single().Value.Should().Be("b");
				}

				[Test]
				public void ExceptionInPropertyGetter_Should_UseExceptionMessageAsValue()
				{
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.Props(logEvent, new TestGetterExceptionClass());

					// Assert
					logEvent.Details.OfType<Arguments>().Single().Values.Single().Should().Be((nameof(TestGetterExceptionClass.Text), "message"));
				}

				[Test]
				public void ExceptionInPropertyToStringWhileStringifying_Should_UseExceptionMessageAsValue()
				{
					var logEvent = createEvent();

					// Act
					ArgumentsExtensions.Props(logEvent, new TestToStringExceptionClass(), stringify: true);

					// Assert
					logEvent.Details.OfType<Arguments>().Single().Values.Single().Should().Be((nameof(TestToStringExceptionClass.Property), "message"));
				}
			}
		}
	}
}
