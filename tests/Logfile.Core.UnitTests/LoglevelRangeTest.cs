using NUnit.Framework;
using FluentAssertions;

namespace Logfile.Core.UnitTests
{
	class LoglevelRangeTest
	{
		[Test]
		public void ConstructorFromTo_Should_SetProperties()
		{
			// "from"/"lower" equals more detailed loglevels, "to"/"higher" equals more critical loglevels
			var range = new LoglevelRange<StandardLoglevel>(StandardLoglevel.Debug, StandardLoglevel.Error);
			range.From.Should().Be(StandardLoglevel.Debug);
			range.To.Should().Be(StandardLoglevel.Error);
		}

		[Test]
		public void ConstructorFromToWithInvertedOrder_Should_SetProperties()
		{
			// "from"/"lower" equals more detailed loglevels, "to"/"higher" equals more critical loglevels
			var range = new LoglevelRange<StandardLoglevel>(StandardLoglevel.Error, StandardLoglevel.Debug);
			range.From.Should().Be(StandardLoglevel.Debug);
			range.To.Should().Be(StandardLoglevel.Error);
		}

		[Test]
		public void ConstructorLoglevel_Should_SetProperties()
		{
			var range = new LoglevelRange<StandardLoglevel>(StandardLoglevel.Information, StandardLoglevel.Information);
			range.From.Should().Be(StandardLoglevel.Information);
			range.To.Should().Be(StandardLoglevel.Information);
		}

		[Test]
		public void CheckIsCoveredForLowerLoglevel_ShouldReturn_True()
		{
			var range = new LoglevelRange<StandardLoglevel>(StandardLoglevel.Debug, StandardLoglevel.Error);
			range.CheckIsCovered(StandardLoglevel.Debug).Should().BeTrue();
		}

		[Test]
		public void CheckIsCoveredForUpperLoglevel_ShouldReturn_True()
		{
			var range = new LoglevelRange<StandardLoglevel>(StandardLoglevel.Debug, StandardLoglevel.Error);
			range.CheckIsCovered(StandardLoglevel.Error).Should().BeTrue();
		}

		[Test]
		public void CheckIsCoveredForIntermediateLoglevel_ShouldReturn_True()
		{
			var range = new LoglevelRange<StandardLoglevel>(StandardLoglevel.Debug, StandardLoglevel.Error);
			range.CheckIsCovered(StandardLoglevel.Warning).Should().BeTrue();
		}

		[Test]
		public void CheckIsCoveredForAboveUpperLoglevel_ShouldReturn_False()
		{
			var range = new LoglevelRange<StandardLoglevel>(StandardLoglevel.Debug, StandardLoglevel.Error);
			range.CheckIsCovered(StandardLoglevel.Critical).Should().BeFalse();
		}

		[Test]
		public void CheckIsCoveredForBelowLowerLoglevel_ShouldReturn_False()
		{
			var range = new LoglevelRange<StandardLoglevel>(StandardLoglevel.Debug, StandardLoglevel.Error);
			range.CheckIsCovered(StandardLoglevel.Trace).Should().BeFalse();
		}
	}
}
