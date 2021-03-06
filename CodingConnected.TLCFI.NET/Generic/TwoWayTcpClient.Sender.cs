﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Core.Data;
using CodingConnected.TLCFI.NET.Core.Data;

namespace CodingConnected.TLCFI.NET.Core.Generic
{
    public sealed partial class TwoWayTcpClient
    {
        private sealed class Sender
        {
            internal event EventHandler<string> DataSent;
            internal event EventHandler Disconnected;

            internal bool Disposed;

            internal async Task SendDataAsync(string data, CancellationToken token)
            {
                try
                {
                    if (Disposed)
                        return;

                    _data = string.Copy(data);
                    await _writer.WriteAsync(_data);
                    _writer.Flush();
                    if(!_data.Contains("Alive") || TLCFIDataProvider.Default.Settings.LogAliveTrace)
                    {
                        _logger.Trace(" --> {0}", _data);
                    }
                    DataSent?.Invoke(this, _data);
                }
                catch (IOException)
                {
                    Disconnected?.Invoke(this, new EventArgs());
                }
                catch
                {
                    Disconnected?.Invoke(this, new EventArgs());
                }
            }

            internal Sender(NetworkStream stream)
            {
                _writer = new StreamWriter(stream);
            }

            private readonly StreamWriter _writer;
            private string _data;
        }
    }
}
