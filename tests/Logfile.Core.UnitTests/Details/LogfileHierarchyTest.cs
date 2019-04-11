using FluentAssertions;
using Logfile.Core.Details;
using NUnit.Framework;
using System.Linq;

namespace Logfile.Core.UnitTests.Details
{
	class LogfileHierarchyTest
	{
		[Test]
		public void ConstructorHierarchyNull_Should_SetEmptyList()
		{
			new LogfileHierarchy(null).Hierarchy.Should().BeEmpty();
		}

		[Test]
		public void Constructor_Should_SetProperties()
		{
			var detail = new LogfileHierarchy(new string[] { "proxy1", "proxy2" });
			detail.Hierarchy.First().Should().Be("proxy1");
			detail.Hierarchy.Last().Should().Be("proxy2");
		}

		[Test]
		public void ToString_ShouldReturn_ConcatenatedNames()
		{
			var detail = new LogfileHierarchy(new string[] { "proxy1", "proxy2" });
			detail.ToString().Should().Be("proxy1.proxy2");
		}
	}
}
