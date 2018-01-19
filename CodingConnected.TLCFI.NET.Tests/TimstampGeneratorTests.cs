using System;
using CodingConnected.TLCFI.NET.Core.Tools;
using NUnit.Framework;

namespace CodingConnected.TLCFI.NET.Core.Tests
{
	[TestFixture]
	public class TimstampGeneratorTests
	{
		[Test]
		public void ProgramStarts_GetTimestampCalled_ReturnsTimeSince1970()
		{
			var t = TimestampGenerator.GetTimestamp();
			var timenow = DateTime.Now;

			// Check year and days
			Assert.AreEqual(timenow.Year - 1970, (int)(t / 31557600000));
			// TODO need to round days upwards
			Assert.AreEqual(
				timenow.DayOfYear - 1 + Math.Round((timenow.Year - 1970) * 365.25) +
				((((timenow.Year - 1970) * 365.25) % 1.0) > 0 ? 1 : 0), (int)(t / 86400000));
		}
	}
}