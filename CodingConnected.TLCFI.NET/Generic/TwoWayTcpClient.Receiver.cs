using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodingConnected.TLCFI.NET.Generic
{
    public sealed partial class TwoWayTcpClient
    {
        private sealed class Receiver
        {
            internal event EventHandler<string> DataReceived;
            internal event EventHandler Disconnected;

            internal bool Disposed;

            internal Receiver(NetworkStream stream)
            {
                _stream = stream;
                _reader = new StreamReader(stream, Encoding.ASCII);
                var thread = new Thread(Run);
                thread.Start();
            }

            byte[] receiveBuffer = new byte[32768];

            private void Run()
            {
                var message = 0;
                var sb = new StringBuilder();
                while (true)
                {
                    try
                    {
                        if (Disposed)
                            return;

                        var bytesRead = _stream.Read(receiveBuffer, 0, 32768);
                        if (bytesRead < 0)
                        {
                            // Read returns 0 if the client closes the connection
                            throw new IOException();
                        }

                        string dataFromClient = Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead);
                        foreach (var c in dataFromClient)
                        {
                            switch (c)
                            {
                                case '\n':
                                case '\r':
                                case ' ':
                                case '\t':
                                    continue;
                                case '{':
                                    message++;
                                    sb.Append(c);
                                    break;
                                default:
                                    if (message > 0 && c == '}')
                                    {
                                        sb.Append(c);
                                        message--;
                                        if (message == 0)
                                        {
                                            var data = sb.ToString();
                                            sb.Clear();
                                            _logger.Trace(" <-- {0}", data);
                                            DataReceived?.Invoke(this, data);
                                        }
                                    }
                                    else if (message > 0)
                                    {
                                        sb.Append(c);
                                    }
                                    break;
                            }
                        }
                    }
                    catch (IOException)
                    {
                        break;
                    }
                    catch (ThreadAbortException)
                    {
                        break;
                    }
                }
                Disconnected?.Invoke(this, new EventArgs());
            }

            private StreamReader _reader;
            private readonly NetworkStream _stream;
        }
    }
}
