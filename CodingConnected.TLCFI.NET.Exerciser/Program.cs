using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CodingConnected.TLCFI.NET.Core.Generic;

namespace TLCFI.NET.Exerciser
{
    internal static class Program
    {
        private static TcpListener Listener;

        private static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                args = new[] { "10.0.0.1", "11501", "config.xml" };
                ///Console.WriteLine("Please provide two command line arguments:");
                ///Console.WriteLine("- ip address, for example: 10.1.2.3");
                ///Console.WriteLine("- port, for example: 11051");
                ///Console.WriteLine("- config file, for example: \"c:\\temp\\config.xml\"");
                ///return;
            }

            IPEndPoint ipep;
            try
            {
                ipep = new IPEndPoint(IPAddress.Parse(args[0]), Int32.Parse(args[1]));
            }
            catch
            {
                Console.WriteLine("No valid ip and port provided for listener");
                return;
            }

            if (!File.Exists(args[2]))
            {
                Console.WriteLine("Could not load config file " + args[2]);
                return;
            }
            var serializer = new XmlSerializer(typeof(TLCFIExerciserSetup));
            TLCFIExerciserSetup exerciserSetup = null;
            try
            {
                using (var reader = new StreamReader(args[2]))
                {
                    exerciserSetup = (TLCFIExerciserSetup) serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not properly deserialize config file: " + e);
                return;
            }

            Console.WriteLine("TLCFI.NET exerciser all set up; now listening at {0}:{1}. Press any key to exit.", args[0], args[1]);

            Listener = new TcpListener(ipep);
            Listener.Start();
            var tcpclient = Listener.AcceptTcpClient();
            var client = new TwoWayTcpClient(tcpclient);
            var exerciser = new TLCFIExerciser(client, exerciserSetup);
            Console.ReadKey();
        }
    }
}
