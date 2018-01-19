using System.Threading;
using CodingConnected.TLCFI.NET.Core.Tools;
using NUnit.Framework;

namespace CodingConnected.TLCFI.NET.Core.Tests
{
	[TestFixture]
	public class TicksGeneratorTests
	{
		[Test]
		public void ProgramDelays100ms_TicksGeneratorDefault_GetCurrentTicksReturns100()
		{
			TicksGenerator.Default.Reset();

			TicksGenerator.Default.GetCurrentTicks();
			Thread.Sleep(100);
			var t = TicksGenerator.Default.GetCurrentTicks();

			// note: allow 10 ms divergence, cause thread.sleep is inaccurate
			Assert.AreEqual(100, t, 10);
		}

		[Test]
		public void ProgramDelays100ms_TicksGeneratorAlmostMax_GetCurrentTicksReturnsNear0()
		{
			TicksGenerator.Default.Reset();

			TicksGenerator.Default.GetCurrentTicks();
			var prop = TicksGenerator.Default.GetType()
				.GetField("_overflow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			prop.SetValue(TicksGenerator.Default, 4294967290);			
			Thread.Sleep(100);
			var t = TicksGenerator.Default.GetCurrentTicks();

			// note: allow 10 ms divergence, cause thread.sleep is inaccurate
			Assert.AreEqual(95, t, 10);
		}
	}
}
