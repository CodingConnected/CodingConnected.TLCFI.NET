using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CodingConnected.TLCFI.NET.Core.Generic;
using NUnit.Framework;

namespace CodingConnected.TLCFI.NET.Core.Tests
{
	[TestFixture]
	public class TwoWayTcpClientTests
	{
		[Test]
		public void ClientConnected_SendAndReceivesInSeperateThreads_AllDataTransfered()
		{
			var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 53872);
			listener.Start();
			var tcpclient = new TcpClient();
			tcpclient.Connect("127.0.0.1", 53872);
			var client = new TwoWayTcpClient(tcpclient);
			var received = new StringBuilder();
			var sent = new StringBuilder();
			client.DataReceived += (s, e) =>
			{
				received.Append(e);
			};
			var client2 = listener.AcceptTcpClient();
			client2.ReceiveBufferSize = 32768;
			client2.SendBufferSize = 32768;

			var t1 = new Thread(async () =>
			{
				for (int i = 0; i < 10; ++i)
				{
					Thread.Sleep(25);
					await client.SendDataAsync("{test" + i + "}\n", CancellationToken.None);
				}
			});
			var t2 = new Thread(() =>
			{
				using (var nstr = client2.GetStream())
				{
					using (var strw = new StreamWriter(nstr))
					{
						strw.AutoFlush = true;
						using (var strr = new StreamReader(nstr))
						{
							for (int i = 0; i < 10; ++i)
							{
								Thread.Sleep(25);
								strw.Write("{test" + i + "}");
								sent.Append(strr.ReadLine());
							}
						}
					}
				}
			});
			t1.Start();
			t2.Start();
			Thread.Sleep(500);
			client2.Close();
			client.Dispose();

			var result = "";
			for (int i = 0; i < 10; ++i)
			{
				result = result + ("{test" + i) + "}";
			}
			Assert.AreEqual(result, received.ToString());
			Assert.AreEqual(result, sent.ToString());
		}
	}
}
