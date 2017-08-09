using System;
using System.IO;
using System.Threading;
using CodingConnected.TLCFI.NET.Client;
using CodingConnected.TLCFI.NET.Client.Data;

namespace CodingConnected.TLCFI.NET.TestClient
{
    public static class Program
    {
        private static readonly string _curDir = AppDomain.CurrentDomain.BaseDirectory;

        private static void Main(string[] args)
        {
            var mainTokenSource = new CancellationTokenSource();
            var fiConfig = Generic.XmlFileSerializer.Deserialize<TLCFIClientConfig>(Path.Combine(_curDir, "fiClientConfig.xml"));
            var fiClient = new TLCFIClient(fiConfig, mainTokenSource.Token);
            var controller = new ControllerExample(fiClient);
        }
    }
}
