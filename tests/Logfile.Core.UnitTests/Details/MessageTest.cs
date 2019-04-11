using FluentAssertions;
using Logfile.Core.Details;
using NUnit.Framework;
using System;
using System.Linq;

namespace Logfile.Core.UnitTests.Details
{
	class MessageTest
	{
		#region Add message

		[Test]
		public void AddMessageWithNullEvent_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => MessageExtensions.Msg<StandardLoglevel>(null, null, null));
		}

		[Test]
		public void AddMessageWithNullTextAndNullArguments_Should_Ignore()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);

			MessageExtensions.Msg(logEvent, null, null).Should().BeSameAs(logEvent);

			var message = logEvent.Details.OfType<Message>().Should().BeEmpty();
		}

		[Test]
		public void AddMessageWithFormatStringTextAndNullArguments_Should_SucceedWithoutArguments()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var text = "Message text with arg #1 ({0}) and arg #2 ({1}).";

			MessageExtensions.Msg(logEvent, text).Should().BeSameAs(logEvent);

			var message = logEvent.Details.OfType<Message>().Single();
			message.Text.Should().Be(text);
			message.StringArguments.Should().BeEmpty();
			message.ToString().Should().Be("Message text with arg #1 ({0}) and arg #2 ({1}).");
		}

		[Test]
		public void AddMessageWithNonFormatStringTextAndArguments_Should_StoreArgumentsButSucceed()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var text = "Message text";
			var arg1 = 1;
			var arg2 = true;

			MessageExtensions.Msg(logEvent, text, arg1, arg2).Should().BeSameAs(logEvent);

			var message = logEvent.Details.OfType<Message>().Single();
			message.Text.Should().Be(text);
			message.StringArguments.Count().Should().Be(2);
			message.StringArguments.Should().Contain(arg1.ToString());
			message.StringArguments.Should().Contain(arg2.ToString());
			message.ToString().Should().Be("Message text");
		}

		[Test]
		public void AddMessageWithFormatStringTextAndArguments_Should_Succeed()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var text = "Message text with arg #1 ({0}) and arg #2 ({1}).";
			var arg1 = 1;
			var arg2 = true;

			MessageExtensions.Msg(logEvent, text, arg1, arg2).Should().BeSameAs(logEvent);

			var message = logEvent.Details.OfType<Message>().Single();
			message.Text.Should().Be(text);
			message.StringArguments.Count().Should().Be(2);
			message.StringArguments.Should().Contain(arg1.ToString());
			message.StringArguments.Should().Contain(arg2.ToString());
			message.ToString().Should().Be("Message text with arg #1 (1) and arg #2 (True).");
		}

		[Test]
		public void AddMultipleMessages_Should_KeepSeparateMessagesInOrderOfCreation()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var text1 = "First message";
			var text2 = "Second message";
			var text3 = "Third message";

			MessageExtensions.Msg(logEvent, text1).Should().BeSameAs(logEvent);
			MessageExtensions.Msg(logEvent, text2).Should().BeSameAs(logEvent);
			MessageExtensions.Msg(logEvent, text3).Should().BeSameAs(logEvent);

			var messages = logEvent.Details.OfType<Message>().Select(m => m.Text);
			messages.Count().Should().Be(3);
			messages.ElementAt(0).Should().Be(text1);
			messages.ElementAt(1).Should().Be(text2);
			messages.ElementAt(2).Should().Be(text3);
		}

		class ArgumentToStringException
		{
			public override string ToString()
			{
				throw new InvalidOperationException();
			}
		}

		[Test]
		public void AddMessageWithFormatStringTextAndArgumentsOfWhichOneFailsAtToString_Should_SucceedAndReplaceErroneousArgumentsWithTypeName()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var text1 = "Message text with arg #1 ({0}) and arg #2 ({1}).";
			var arg11 = 1;
			var arg12 = new ArgumentToStringException();
			var text2 = "Second message";

			MessageExtensions.Msg(logEvent, text1, arg11, arg12).Should().BeSameAs(logEvent);
			MessageExtensions.Msg(logEvent, text2).Should().BeSameAs(logEvent);

			var messages = logEvent.Details.OfType<Message>();
			messages.Count().Should().Be(2);
			messages.ElementAt(0).ToString().Should().Be(string.Format(text1, arg11, arg12.GetType().Name));
			messages.ElementAt(1).ToString().Should().Be("Second message");
		}

		#endregion

		#region ToString

		[Test]
		public void MessageToStringWithFormatStringTextAndTooManyArguments_Should_AlsoStoreSuperfluousArgumentsButNotPrintIt()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var text = "Message text with arg #1 ({0}) and arg #2 ({1}).";
			var arg1 = 1;
			var arg2 = true;
			var arg3 = 123.45;

			MessageExtensions.Msg(logEvent, text, arg1, arg2, arg3).Should().BeSameAs(logEvent);

			var message = logEvent.Details.OfType<Message>().Single();
			message.Text.Should().Be(text);
			message.StringArguments.Count().Should().Be(3);
			message.StringArguments.Should().Contain(arg1.ToString());
			message.StringArguments.Should().Contain(arg2.ToString());
			message.StringArguments.Should().Contain(arg3.ToString());
			message.ToString().Should().Be("Message text with arg #1 (1) and arg #2 (True).");
		}

		[Test]
		public void MessageToStringWithFormatStringTextAndTooFewArguments_Should_FillInAvailableArgumentsAndLeaveOtherPlaceholdersUntouched()
		{
			var logEvent = new LogEvent<StandardLoglevel>(_ => { }, StandardLoglevel.Warning);
			var text = "Message text with arg #1 ({0}) and arg #2 ({1}).";
			var arg1 = 1;

			MessageExtensions.Msg(logEvent, text, arg1).Should().BeSameAs(logEvent);

			var message = logEvent.Details.OfType<Message>().Single();
			message.Text.Should().Be(text);
			message.StringArguments.Count().Should().Be(1);
			message.StringArguments.Should().Contain(arg1.ToString());
			message.ToString().Should().Be("Message text with arg #1 ({0}) and arg #2 ({1}). {\"1\"}");
		}

		#endregion
	}
}
