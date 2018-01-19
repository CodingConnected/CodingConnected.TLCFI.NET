using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Client.Data;
using CodingConnected.TLCFI.NET.Client.Session;
using CodingConnected.TLCFI.NET.Core.Exceptions;
using NSubstitute;
using NUnit.Framework;

namespace CodingConnected.TLCFI.NET.Client.Tests
{
	[TestFixture]
	public class TLCFIClientTests
	{
		[Test]
		public void TLCFIClientWithoutAutoreconnect_StartSessionAsync_StopsAfter1Try()
		{
			var sessionManager = Substitute.For<ITLCFIClientSessionManager>();
			var tryCounter = 0;
			sessionManager.
				When(x => x.GetNewSession(Arg.Any<IPEndPoint>(), Arg.Any<TLCFIClientStateManager>(), Arg.Any<CancellationToken>())).
				Do(c =>
				{
					Task.Delay(20, c.Arg<CancellationToken>());
					tryCounter++;
				});
			sessionManager.GetNewSession(Arg.Any<IPEndPoint>(), Arg.Any<TLCFIClientStateManager>(), Arg.Any<CancellationToken>()).Returns((TLCFIClientSession)null);

			var cts = new CancellationTokenSource();
			var client = new TLCFIClient(TLCFITestHelpers.GetClientConfig(55000, false), cts.Token);
			client.OverrideDefaultSessionManager(sessionManager);
			var watcher = new Stopwatch();

			watcher.Start();
			var t1 = Task.Run(async () =>
			{
				await client.StartSessionAsync(cts.Token);
			}, cts.Token);
			var t2 = Task.Delay(50, cts.Token);
			Task.WaitAny(t1, t2);
			cts.Cancel();
			watcher.Stop();

			Assert.AreEqual(tryCounter, 1);
		}

		[Test]
		public void TLCFIClientWithAutoreconnect_StartSessionAsync_WillRetryUponFail()
		{
			var sessionManager = Substitute.For<ITLCFIClientSessionManager>();
			var tryCounter = 0;
			sessionManager.
				When(x => x.GetNewSession(Arg.Any<IPEndPoint>(), Arg.Any<TLCFIClientStateManager>(), Arg.Any<CancellationToken>())).
				Do(c =>
				{
					Task.Delay(20, c.Arg<CancellationToken>());
					tryCounter++;
				});
			sessionManager.GetNewSession(Arg.Any<IPEndPoint>(), Arg.Any<TLCFIClientStateManager>(), Arg.Any<CancellationToken>()).Returns((TLCFIClientSession)null);

			var cts = new CancellationTokenSource();
			var client = new TLCFIClient(TLCFITestHelpers.GetClientConfig(55000, autoreconnect: true), cts.Token);
			client.OverrideDefaultSessionManager(sessionManager);
			
			var t1 = Task.Run(async () =>
			{
				await client.StartSessionAsync(cts.Token);
			}, cts.Token);
			var t2 = Task.Delay(50, cts.Token);
			Task.WaitAny(t1, t2);
			cts.Cancel();
			
			Assert.Greater(tryCounter, 1);
		}

		[Test]
		public void TLCFIClientStartSessionAsync_ExceptionInSessionManager_RetriesIfNonFatal()
		{
			var sessionManager = Substitute.For<ITLCFIClientSessionManager>();
			var tryCounter = 0;
			sessionManager.
				When(x => x.GetNewSession(Arg.Any<IPEndPoint>(), Arg.Any<TLCFIClientStateManager>(), Arg.Any<CancellationToken>())).
				Do(c =>
				{
					Task.Delay(10, c.Arg<CancellationToken>());
					tryCounter++;
				});
			sessionManager.GetNewSession(Arg.Any<IPEndPoint>(), Arg.Any<TLCFIClientStateManager>(), Arg.Any<CancellationToken>()).Returns(new TLCFIClientSession(new TLCFIClientStateManager(), new IPEndPoint(0, 0), CancellationToken.None));
			var initializer = Substitute.For<ITLCFIClientInitializer>();
			initializer.InitializeSession(
				Arg.Any<TLCFIClientSession>(),
				Arg.Any<TLCFIClientConfig>(),
				Arg.Any<TLCFIClientStateManager>(),
				Arg.Any<CancellationToken>()).Returns(x => { throw new TLCFISessionException("", false); });

			var cts = new CancellationTokenSource();
			var client = new TLCFIClient(TLCFITestHelpers.GetClientConfig(55000, autoreconnect: true), cts.Token);
			client.OverrideDefaultSessionManager(sessionManager);
			client.OverrideDefaultInitializer(initializer);

			var t1 = Task.Run(async () =>
			{
				await client.StartSessionAsync(cts.Token);
			}, cts.Token);
			var t2 = Task.Delay(50, cts.Token);
			Task.WaitAny(t1, t2);
			cts.Cancel();

			Assert.Greater(tryCounter, 1);
		}

		[Test]
		public void TLCFIClientStartSessionAsync_ExceptionInSessionManager_StopsIfFatal()
		{
			var sessionManager = Substitute.For<ITLCFIClientSessionManager>();
			var tryCounter = 0;
			sessionManager.
				When(x => x.GetNewSession(Arg.Any<IPEndPoint>(), Arg.Any<TLCFIClientStateManager>(), Arg.Any<CancellationToken>())).
				Do(c =>
				{
					Task.Delay(10, c.Arg<CancellationToken>());
					tryCounter++;
				});
			sessionManager.GetNewSession(Arg.Any<IPEndPoint>(), Arg.Any<TLCFIClientStateManager>(), Arg.Any<CancellationToken>()).Returns(new TLCFIClientSession(new TLCFIClientStateManager(), new IPEndPoint(0, 0), CancellationToken.None));

			var initializer = Substitute.For<ITLCFIClientInitializer>();
			initializer.InitializeSession(
				Arg.Any<TLCFIClientSession>(),
				Arg.Any<TLCFIClientConfig>(),
				Arg.Any<TLCFIClientStateManager>(),
				Arg.Any<CancellationToken>()).Returns(x =>
			{
				throw new TLCFISessionException("", true);
			});

			var cts = new CancellationTokenSource();
			var client = new TLCFIClient(TLCFITestHelpers.GetClientConfig(55000, autoreconnect: true), cts.Token);
			client.OverrideDefaultSessionManager(sessionManager);
			client.OverrideDefaultInitializer(initializer);
			
			var t1 = Task.Run(async () =>
			{
				await client.StartSessionAsync(cts.Token);
			}, cts.Token);
			var t2 = Task.Delay(50, cts.Token);
			Task.WaitAny(t1, t2);
			cts.Cancel();

			Assert.AreEqual(tryCounter, 1);
		}
	}
}
