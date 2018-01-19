using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CodingConnected.JsonRPC;
using CodingConnected.TLCFI.NET.Core.Generic;
using CodingConnected.TLCFI.NET.Core.Tools;
using Newtonsoft.Json;
using NLog;

namespace TLCFI.NET.Exerciser
{
    public class TLCFIExerciser
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly DateTime _timeStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly Regex _jsonRpcMethodRegex = new Regex(@"['""]method['""]", RegexOptions.Compiled);

        private readonly TwoWayTcpClient _client;
        private readonly TLCFIExerciserSetup _setup;
        private readonly Timer _aliveTimer;

        #endregion // Fields

        #region Properties

        public static uint CurrentTicks => TicksGenerator.Default.GetCurrentTicks();
        public static ulong CurrentTime => (uint)DateTime.UtcNow.Subtract(_timeStart).TotalMilliseconds;
        
        #endregion // Properties

        #region Private Methods

        private async void ProcessReceivedData(object sender, string data)
        {
            JsonRpcRequest request;
            if (!_jsonRpcMethodRegex.IsMatch(data))
                return;

            try
            {
                request = JsonConvert.DeserializeObject<JsonRpcRequest>(data);
            }
            catch
            {
                _logger.Error("Data received is not a valid JSON-RPC request. Data: {0}", data);
                return;
            }

            TLCFIExerciserSetupElement response;
            string rpcresponse;
            switch (request.Method)
            {
                case "Alive":
                    rpcresponse = "{\"jsonrpc\":\"2.0\",\"result\":" + request.Params + ",\"id\":" + request.Id +"}";
                    await _client.SendDataAsync(rpcresponse, CancellationToken.None);
                    break;
                case "UpdateState":
                    // TODO
                    break;
                case "Deregister":
                    rpcresponse = "{\"jsonrpc\":\"2.0\",\"result\":{},\"id\":" + request.Id + "}";
                    await _client.SendDataAsync(rpcresponse, CancellationToken.None);
                    break;
                case "Register":
                    response = _setup.Elements.FirstOrDefault(x => x.Method == "Register");
                    if (response != null)
                    {
                        rpcresponse = response.Response.Replace("__ID__", request.Id.ToString());
                        rpcresponse = rpcresponse.Replace("__TICKS__", CurrentTicks.ToString());
                        await _client.SendDataAsync(rpcresponse, CancellationToken.None);
                    }
                    break;
                case "ReadMeta":
                    var rmtype = Regex.Match(request.Params.ToString(), @".*""type""\s*:\s*(?<type>[0-9]+).*");
                    response = _setup.Elements.FirstOrDefault(
                                x => x.Method == "ReadMeta" &&
                                     x.Type == rmtype.Groups["type"].Value);
                    if (response != null)
                    {
                        rpcresponse = response.Response.Replace("__ID__", request.Id.ToString());
                        rpcresponse = rpcresponse.Replace("__TICKS__", CurrentTicks.ToString());
                        await _client.SendDataAsync(rpcresponse, CancellationToken.None);
                    }
                    break;
                case "Subscribe":
                    var sctype = Regex.Match(request.Params.ToString(), @".*""type""\s*:\s*(?<type>[0-9]+).*");
                    response = _setup.Elements.FirstOrDefault(
                                x => x.Method == "Subscribe" &&
                                     x.Type == sctype.Groups["type"].Value);
                    if (response != null)
                    {
                        rpcresponse = response.Response.Replace("__ID__", request.Id.ToString());
                        rpcresponse = rpcresponse.Replace("__TICKS__", CurrentTicks.ToString());
                        await _client.SendDataAsync(rpcresponse, CancellationToken.None);
                    }
                    break;
                default:
                    _logger.Warn("Unknown JSON-RPC method called by client: {0}", request.Method);
                    break;
            }
        }

        #endregion // Private Methods

        #region Constructor

        public TLCFIExerciser(TwoWayTcpClient client, TLCFIExerciserSetup setup)
        {
            _client = client;
            _setup = setup;

            var id = 0;
            _aliveTimer = new Timer(async state =>
            {
                await _client.SendDataAsync("{\"id\":" + id++ + ",\"jsonrpc\":\"2.0\"," +
                                      "\"method\":\"Alive\",\"params\":{\"ticks\":" + CurrentTicks + ",\"time\":" + CurrentTime +"}}", CancellationToken.None);
            }, null, 2000, 2000);

            _client.DataReceived += ProcessReceivedData;
        }

        #endregion // Constructor
    }
}