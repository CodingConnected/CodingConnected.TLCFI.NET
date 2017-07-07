using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.JsonRPC;
using CodingConnected.TLCFI.NET.Client.Data;
using CodingConnected.TLCFI.NET.Generic;
using CodingConnected.TLCFI.NET.Models.Generic;
using CodingConnected.TLCFI.NET.Models.TLC;
using CodingConnected.TLCFI.NET.Proxies;
using NLog;

namespace CodingConnected.TLCFI.NET.Client.Session
{
    /// <summary>
    /// Represents the session with a remote TLC Facilities.
    /// This class handled TLC-FI JSON-RPC calls and monitors session health via alive checking.
    /// The class responds to JSON-RPC calls by calling methods and setting state in the CLABase class.
    /// </summary>
    internal class TLCFIClientSessionJsonRpcHandler : ITLCFIClient
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly JsonRpcService _service;
        private readonly TLCFIClientStateManager _stateManager;
        private readonly TLCProxy _tlcProxy;
        private readonly TLCFIClientSessionState _sessionState;
        private readonly CancellationToken _sessionCancellationToken;
        private readonly Regex _jsonRpcMethodRegex = new Regex(@"['""]method['""]", RegexOptions.Compiled);

        #endregion // Fields

        #region Events

        public event EventHandler ReceivedAlive;

        public event EventHandler<ObjectStateUpdate> UpdateStateCalled; 

        #endregion // Events

        #region Public Methods

        public async Task SetReqControlStateAsync(ControlState state)
        {
            _stateManager.ControlSession.ReqControlState = state;
            try
            {
                await _tlcProxy.UpdateStateAsync(GetObjectStateUpdateGroup(_stateManager.Session), _sessionCancellationToken);
            }
            catch (TaskCanceledException)
            {

            }
            _stateManager.Session.ResetChanged();
        }

        public async Task SetIntersectionReqStateAsync(IntersectionControlState state)
        {
            _stateManager.Intersection.ReqState = state;
            try
            {
                await _tlcProxy.UpdateStateAsync(GetObjectStateUpdateGroup(_stateManager.Intersection), _sessionCancellationToken);
            }
            catch (TaskCanceledException)
            {

            }
            _stateManager.Intersection.ResetChanged();
        }

        #endregion // Public Methods

        #region Private Methods

        private ObjectStateUpdateGroup GetObjectStateUpdateGroup(TLCObjectBase obj)
        {
            var objectStateUpdate = new[]
            {
                new ObjectStateUpdate()
                {
                    Objects = new ObjectReference()
                    {
                        Ids = new[] { obj.Id },
                        Type = obj.ObjectType
                    },
                    States = new[]
                    {
                        obj.GetState()
                    }
                }
            };
            return new ObjectStateUpdateGroup()
            {
                Ticks = TLCFIClient.CurrentTicks,
                Update = objectStateUpdate
            };
        }

        #endregion // Private Methods

        #region ITLCFIApplication

        [JsonRpcMethod]
        public AliveObject Alive(AliveObject alive)
        {
            ReceivedAlive?.Invoke(this, EventArgs.Empty);
            //return _sessionState.SystemsAlive ? alive : null;
            return alive;
        }

        [JsonRpcMethod]
        public void NotifyEvent(ObjectEvent objectevent)
        {
            _logger.Info("NotifyEvent was called via Json-Rpr: " + objectevent);
            // TODO: handle events
        }

        [JsonRpcMethod]
        public void UpdateState(ObjectStateUpdateGroup objectstateupdategroup)
        {
            var updategroup = objectstateupdategroup.Update;
            foreach (var update in updategroup)
            {
                var objects = update.Objects;
                var states = update.States;
                if (objects.Ids.Length != states.Length)
                {
                    throw new JsonRpcException((int)ProtocolErrorCode.Error, "List of object ids and list of states were of different lengths.", null);
                }
                if (objects.Ids.Length == 0)
                {
                    throw new JsonRpcException((int)ProtocolErrorCode.Error, "List of objects is empty.", null);
                }
                UpdateStateCalled?.Invoke(this, update);
            }
        }

        [JsonRpcMethod]
        public ObjectMeta ReadMeta(ObjectReference objectReference)
        {
            throw new JsonRpcException((int)ProtocolErrorCode.Error, "This application does not support the ReadMeta method; ReadMeta should be called on the actual TLC Facilities only", null);
        }

        #endregion // ITLCFIApplication

        #region Constructor

        public TLCFIClientSessionJsonRpcHandler(
            TLCFIClientStateManager stateManager, 
            TLCProxy tlcProxy,
            TLCFIClientSessionState sessionState,
            TwoWayTcpClient tcpClient,
            CancellationToken token)
        {
            _stateManager = stateManager;
            _tlcProxy = tlcProxy;
            _sessionCancellationToken = token;
            _sessionState = sessionState;
            tcpClient.DataReceived += async (o, e) =>
            {
                if (!_jsonRpcMethodRegex.IsMatch(e)) return;
                var result = _service?.HandleRpc(e);
                if (result != null)
                {
                    await tcpClient.SendDataAsync(result, token);
                }
            };
            _service = JsonRpcProcedureBinder.Default.GetInstanceService(this);
        }

        #endregion // Constructor
    }
}
