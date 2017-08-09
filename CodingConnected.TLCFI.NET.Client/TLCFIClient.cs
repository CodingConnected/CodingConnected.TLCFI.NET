using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Client.Data;
using CodingConnected.TLCFI.NET.Client.Session;
using CodingConnected.TLCFI.NET.EventsArgs;
using CodingConnected.TLCFI.NET.Exceptions;
using CodingConnected.TLCFI.NET.Models.Generic;
using CodingConnected.TLCFI.NET.Models.TLC;
using CodingConnected.TLCFI.NET.Tools;
using JetBrains.Annotations;
using NLog;
using DateTime = System.DateTime;

namespace CodingConnected.TLCFI.NET.Client
{
    /// <summary>
    /// Represents the TLC-FI client-side.
    /// This class is meant to be consumed by a controller application to be able to
    /// control a remote intersection via the TLC-FI.
    /// </summary>
    public class TLCFIClient
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly DateTime _timeStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly TLCFIClientSessionManager _sessionManager;
        private readonly TLCFIClientInitializer _clientInitializer;
        private readonly TLCFIClientConfig _config;
        private readonly CancellationToken _mainCancellationToken;

        private TLCFIClientSession _session;
        private CancellationTokenSource _sessionCancellationTokenSource;
        private CancellationTokenSource _controlEndingCancellationTokenSource;

        private int _sessionErrorCount = 0;
        private bool _fatalErrorOccured;

        private bool _userWantsControl;
        private IntersectionControlState _userRequestedIntersectionControlState;

        private bool _settingState;

        #endregion // Fields

        #region Private Properties

        private bool FatalErrorOccured
        {
            get => _fatalErrorOccured;
            set
            {
                _fatalErrorOccured = value;
                if (value)
                {
                    _logger.Fatal("The TLCFI client had an error considered fatal; it will no longer make new connections. Check the configuration.");
                }
            }
        }


        #endregion // Private Properties

        #region Public Properties

        [UsedImplicitly]
        public TLCFIClientStateManager StateManager { get; internal set; }

        public static uint CurrentTicks => TicksGenerator.Default.GetCurrentTicks();
        public static ulong CurrentTime => (uint)DateTime.UtcNow.Subtract(_timeStart).TotalMilliseconds;

        public bool Connected => _session?.Connected ?? false;
        public bool Configured => _session?.State?.Configured ?? false;
        public bool SessionInControl => _session?.State?.SessionControl ?? false;
        public bool IntersectionInControl => _session?.State?.IntersectionControl ?? false;
        public double AvgResponseToRequestsTime => StateManager.AvgResponseToRequestsTime;

        /// <summary>
        /// Can be used to indicate wether or not external system are OK
        /// If set to false, this will halt sending alive messages
        /// </summary>
        public bool SystemsAlive
        {
            get => _session?.State?.SystemsAlive ?? false;
            set
            {
                if (_session?.State == null)
                {
                    return;
                }
                _session.State.SystemsAlive = value;
            }
        }

        #endregion // Public Properties

        #region Events

        /// <summary>
        /// Raised whenever a detector changes state
        /// </summary>
        [UsedImplicitly]
        public event EventHandler<Detector> DetectorStateChanged;

        /// <summary>
        /// Raised whenever a signal group changes state
        /// </summary>
        [UsedImplicitly]
        public event EventHandler<SignalGroup> SignalGroupStateChanged;

        /// <summary>
        /// Raised whenever an input changes state
        /// </summary>
        [UsedImplicitly]
        public event EventHandler<Input> InputStateChanged;

        /// <summary>
        /// Raised whenever an output changes state
        /// </summary>
        [UsedImplicitly]
        public event EventHandler<Output> OutputStateChanged;

        /// <summary>
        /// Raised when the client has succesfully connected, registered and configured with the TLC
        /// </summary>
        [UsedImplicitly]
        public event EventHandler ClientInitialized;

        /// <summary>
        /// Raised upon definitive loss of control. The argument indicates if this was expected or not.
        /// (true means it was expected)
        /// The client may no longer set ReqState on any object after loosing control
        /// </summary>
        [UsedImplicitly]
        public event EventHandler<bool> LostControl;

        /// <summary>
        /// Raised ending of a session. The argument indicates if this was expected or not 
        /// (true means it was expected)
        /// The client may no longer set ReqState on any object after loosing control
        /// </summary>
        [UsedImplicitly]
        public event EventHandler<bool> SessionEnded;

        /// <summary>
        /// Raised upon definitive loss of control
        /// The client (if of type ControlApplication) may update ReqState on the intersection 
        /// and non-exclusive outputs now. If the state of the intersection is Control, ReqState
        /// of signal groups and exclusive outputs may also be set.
        /// </summary>
        [UsedImplicitly]
        public event EventHandler GotControl;

        /// <summary>
        /// Raised when a request to start control has been received from the TLC
        /// This event is typically raised after a call to RequestSessionStartControl()
        /// Note: this concerns session control, which is independent from intersection state
        /// </summary>
        [UsedImplicitly]
        public event EventHandler StartControlRequestReceived;

        /// <summary>
        /// Raised when a request to end control has been received from the TLC
        /// This event is typically raised after a call to RequestSessionEndControl, or may be raised
        /// when the TLC Facilities has other reason to request ending of control (for example,
        /// handing control to a different client)
        /// Note: this concerns session control, which is independent from intersection state
        /// </summary>
        [UsedImplicitly]
        public event EventHandler EndControlRequestReceived;

        /// <summary>
        /// Raised when the intersection changes state
        /// </summary>
        [UsedImplicitly]
        public event EventHandler<IntersectionControlState> IntersectionStateChanged;

        #endregion // Events

        #region Public Methods

        /// <summary>
        /// Attemps starting a session with a remote TLC Facilities
        /// The method will automatically attempt to reconnect on connection loss, depending on the 
        /// value of AutoReconnect in the config (which is parsed to the constructor)
        /// Upon succesfully starting an initiating a session, the ClientInitialized event will be raised
        /// </summary>
        /// <remarks>This method will not return until CloseSession is called, or a fatal error has occured,
        /// unless AutoReconnect is set to false</remarks>
        public async Task StartSessionAsync(CancellationToken token)
        {
            do
            {
                try
                {
                    _sessionCancellationTokenSource = new CancellationTokenSource();
                    var sessionToken = _sessionCancellationTokenSource.Token;
                    
                    StateManager = new TLCFIClientStateManager();
                    
                    _session = await _sessionManager.GetNewSession(StateManager, sessionToken);
                    if (_session == null)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        continue;
                    }

                    _session.SessionEnded += (o, e) =>
                    {
#warning TODO: this check does not work this way.
                        //if (StateManager?.Session?.SessionType != _config.ApplicationType)
                        //{
                        //    _logger.Fatal("The application type is configured differently in the TLC. Local config: {0}, remote: {1}. Will no longer reconnect.", _config.ApplicationType, StateManager?.Session?.SessionType);
                        //    FatalErrorOccured = true;
                        //}
                        if (StateManager?.ControlSession?.ControlState != null && 
                            StateManager.ControlSession.ControlState.Value == ControlState.Error)
                        {
                            _sessionErrorCount++;
                            if (_sessionErrorCount >= _config.MaxSessionErrorCount)
                            {
                                _logger.Fatal("The session state was set to ControlState.Error {0} times. Will no longer reconnect.", _config.MaxSessionErrorCount);
                                FatalErrorOccured = true;
                            }
                        }
                        _sessionCancellationTokenSource.Cancel();
                    };

                    // Give the TLC some time (in case starting the session takes time)
                    await Task.Delay(_config.RegisterDelayAfterConnecting, sessionToken);

                    try
                    {
                        await _clientInitializer.InitializeSession(_session, StateManager, sessionToken);
                        // set initial state
                        StateManager.InternalSignalGroups.ForEach(x => SignalGroupStateChanged?.Invoke(this, x));
                        StateManager.InternalDetectors.ForEach(x => DetectorStateChanged?.Invoke(this, x));
                        StateManager.InternalInputs.ForEach(x => InputStateChanged?.Invoke(this, x));
                        StateManager.InternalOutputs.ForEach(x => OutputStateChanged?.Invoke(this, x));
                        // subscribe to state changes
                        StateManager.SignalGroupStateChanged += (o, e) => SignalGroupStateChanged?.Invoke(this, e);
                        StateManager.DetectorStateChanged += (o, e) => DetectorStateChanged?.Invoke(this, e);
                        StateManager.InputStateChanged += (o, e) => InputStateChanged?.Invoke(this, e);
                        StateManager.OutputStateChanged += (o, e) => OutputStateChanged?.Invoke(this, e);

                        StateManager.StateChanged += StateManager_StateChanged;

                        _sessionManager.ResetConnectionRetryTimers();

                        StateManager.ControlSession.HasControlStateChanged += OnSessionControlChanged;
                        StateManager.Intersection.ChangedState += OnIntersectionControlChanged;

                        ClientInitialized?.Invoke(this, EventArgs.Empty);
                    }
                    catch (TLCFISessionException e)
                    {
                        if (e.Fatal) FatalErrorOccured = true;
                        await _sessionManager.EndActiveSessionAsync(false);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                    catch (Exception e)
                    {
                        _logger.Error(
                            "Session could not be started and configured correctly and will be closed. See trace for details.");
                        _logger.Trace(e, "Session could not be started and configured correctly:");
                        await _sessionManager.EndActiveSessionAsync(false);
                    }
                    await Task.Delay(-1, sessionToken);
                    StateManager = null;
                }
                catch (TaskCanceledException)
                {
                    
                }
            } while (_config.AutoReconnect && 
                     !FatalErrorOccured && 
                     !token.IsCancellationRequested);
        }

        /// <summary>
        /// Dispends a message to the TLC Facilities, requesting control. If the TLC grants control, which
        /// may occur immediately or (much) later, the StartControlRequestReceived event will be raised.
        /// </summary>
        public async Task RequestSessionStartControl()
        {
            if (StateManager == null)
            {
                _logger.Warn("Unable to complete call to RequestSessionStartControl: StateManager is null. " +
                             "Was StartSessionAsync called and succesfully completed? ");
                return;
            }
            if (StateManager.Session.SessionType != ApplicationType.Control)
            {
                _logger.Warn("Unauthorized call to RequestSessionStartControl: Application of type {0} may not request control. " +
                             "Will not forward request to TLC.", StateManager.Session.SessionType);
                return;
            }
            if (_session == null || !_session.State.Configured)
            {
                _logger.Warn("Incorrect call to RequestSessionStartControl: the client session has not (yet) been configured. " +
                             "Was StartSessionAsync called and succesfully completed? Will NOT forward request to TLC.");
                return;
            }
            _logger.Debug("RequestStartControl called: when configured, will set session ControlState to ControlState.ReadyToControl.");
            _userWantsControl = true;
            if (StateManager.ControlSession.ControlState != null && 
                StateManager.ControlSession.ControlState.Value == ControlState.Offline)
            {
                await _session.SetReqControlStateAsync(ControlState.ReadyToControl);
            }
        }

        /// <summary>
        /// Dispends a message to the TLC Facilities, requesting ending of control. Typically, the TLC will
        /// respond immediately, which will cause the EndControlRequestReceived event to be raised
        /// </summary>
        public async Task RequestSessionEndControl()
        {
            if (StateManager == null)
            {
                _logger.Warn("Unable to complete call to RequestSessionEndControl: StateManager is null. " +
                             "Were StartSessionAsync and RequestSessionStartControl called and succesfully completed? ");
                return;
            }
            if (StateManager.Session.SessionType != ApplicationType.Control)
            {
                _logger.Warn("Unauthorized call to RequestSessionEndControl: Application of type {0} cannot have control. " +
                             "Will not forward request to TLC.", StateManager.Session.SessionType);
                return;
            }
            if (StateManager.ControlSession.ControlState != null && 
                StateManager.ControlSession.ControlState.Value != ControlState.InControl)
            {
                _logger.Warn("Incorrect call to RequestSessionEndControl: the client session does not have control. " +
                             "Were StartSessionAsync and RequestSessionStartControl called and succesfully completed? " +
                             "Will NOT forward request to TLC.");
                return;
            }
            _logger.Debug("RequestEndControl called: will set session ControlState to ControlState.EndControl.");
            _userWantsControl = false;
            await _session.SetReqControlStateAsync(ControlState.EndControl);
        }

        /// <summary>
        /// Dispends a message to the TLC, requesting the intersection state to be changed.
        /// </summary>
        public async Task SetIntersectionReqControlState(IntersectionControlState state)
        {
            if (StateManager == null)
            {
                _logger.Warn("Unable to complete call to SetIntersectionReqControlState: StateManager is null. " +
                             "Were StartSessionAsync and RequestSessionStartControl called and succesfully completed? ");
                return;
            }
            if (StateManager.Session.SessionType != ApplicationType.Control)
            {
                _logger.Warn("Unauthorized call to SetIntersectionReqControlState: Application of type {0} cannot set intersection state. " +
                             "Will not forward request to TLC.", StateManager.Session.SessionType);
                return;
            }
            if (StateManager.ControlSession.ControlState != null &&
                StateManager.ControlSession.ControlState.Value != ControlState.InControl)
            {
                _logger.Warn("Incorrect call to SetIntersectionReqControlState: the client session does not have control. " +
                             "Were StartSessionAsync and RequestStartControl called and succesfully completed? " +
                             "Will not forward request to TLC.");
                return;
            }
            _userRequestedIntersectionControlState = state;
            _logger.Debug("Setting requested state for intersection to {0} in TLC.", state);
            await _session.SetIntersectionReqStateAsync(state);
        }

        /// <summary>
        /// Used to confirm ending of control. This method should be called after the EndControlRequestReceived
        /// event has been raised, once the client is ready to actually release control
        /// </summary>
        public async Task ConfirmEndControl(CancellationToken token)
        {
            try
            {
                if (_controlEndingCancellationTokenSource == null)
                {
                    _logger.Warn("ConfirmEndControl() called, but EndControl was not requested.");
                }
                _controlEndingCancellationTokenSource?.Cancel();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Exception in ConfirmEndControl: ");
            }
        }

        /// <summary>
        /// Will cause states (ReqState) that were changed since the last call to UpdateState to be synchronized 
        /// with the TLC, or do nothing if nothing changed. It will typically be called in a (controller) loop.
        /// Note: this function is not asynchronous, but the backend does handle synchronising state asynchronously, 
        /// and the function will never stall. 
        /// </summary>
        public void UpdateState()
        {
            StateManager.RaiseStateChanged();
            StateManager.ResetStateChanged();
        }

        /// <summary>
        /// Attempts to gracefully end the current session, then disposes of used resources
        /// </summary>
        public async Task EndSessionAsync()
        {
            if (SessionInControl)
            {
                _logger.Info("EndSessionAsync called while in control: calling RequestSessionEndControl().");
                await RequestSessionEndControl();
            }
            await _sessionManager.EndActiveSessionAsync(true);
            _sessionCancellationTokenSource?.Cancel();
            _sessionManager.DisposeActiveSession();
        }

        /// <summary>
        /// If needed, sets the ReqState property of the signalgroup with the given id to reqState
        /// Does nothing if the value of the ReqState property is already as requested
        /// Throws TLCObjectNotFoundException if the id is not found
        /// </summary>
        public void SetSignalGroupReqState(string id, SignalGroupState reqState)
        {
            if (_config.ApplicationType != ApplicationType.Control)
            {
                _logger.Warn("SetSignalGroupReqState() may only be called when ApplicationType is Control");
                return;
            }
            var signalGroup = StateManager.InternalSignalGroups.FirstOrDefault(x => x.Id == id);
            if (signalGroup != null)
            {
                if (!((StateManager.ControlSession.ControlState == ControlState.InControl ||
                       StateManager.ControlSession.ControlState == ControlState.EndControl) &&
                      StateManager.Intersection.State == IntersectionControlState.Control))
                {
                    _logger.Warn("Not in control of intersection; may not set state of signalgroup with id {0}", signalGroup.Id);
                    return;
                }
                // Check previous if a request has been made that has not yet been confirmed
                if (StateManager.RequestedStates.ContainsKey("sg" + signalGroup.Id))
                {
                    _logger.Warn(
                        "While setting ReqState to {0}: still awaiting previous request to set state for signalgroup {1} to {2}; will clear awaiting buffer.",
                        reqState, id, signalGroup.ReqState);
                    StateManager.RequestedStates.Remove("sg" + signalGroup.Id);
                }
                else if (reqState != signalGroup.State)
                {
                    StateManager.RequestedStates.Add("sg" + signalGroup.Id, CurrentTicks);
                }
                else if (reqState == signalGroup.State)
                {
                    _logger.Warn(
                        "While setting ReqState to {0}: signalgroup {1} is already in that state.",
                        reqState, id);
                }
                signalGroup.ReqState = reqState;
                StateManager.SetObjectStateChanged(id, TLCObjectType.SignalGroup);
            }
            else
            {
                _logger.Error("SetSignalGourpReqState: id {0} not found in StateManager instance.", id);
                throw new TLCObjectNotFoundException($"Id {id} not found in StateManager instance.");
            }
        }

        /// <summary>
        /// If needed, sets the ReqState property of the output with the given id to reqState
        /// Does nothing if the value of the ReqState property is already as requested
        /// Throws TLCObjectNotFoundException if the id is not found
        /// </summary>
        public void SetOutputReqState(string id, int reqState)
        {

            if (_config.ApplicationType == ApplicationType.Consumer)
            {
                _logger.Warn("SetOutputReqState() may only be called when ApplicationType is Control or Provider");
                return;
            }
            var output = StateManager.InternalOutputs.FirstOrDefault(x => x.Id == id);
            if (output != null)
            {
                if (output.Exclusive)
                {
                    if (_config.ApplicationType == ApplicationType.Provider)
                    {
                        _logger.Warn(
                            "Output {0} is exclusive; SetOutputReqState() may only be called when ApplicationType is Control",
                            id);
                        return;
                    }
                    if (!((StateManager.ControlSession.ControlState == ControlState.InControl ||
                           StateManager.ControlSession.ControlState == ControlState.EndControl) &&
                          StateManager.Intersection.State == IntersectionControlState.Control))
                    {
                        _logger.Warn("Output {0} is exclusive; SetOutputReqState() may only be called when in control of intersection", output.Id);
                        return;
                    }
                }
                if (StateManager.RequestedStates.ContainsKey("os" + output.Id))
                {
                    _logger.Warn(
                        "While setting ReqState to {0}: still awaiting previous request to set state for output {1} to {2}",
                        reqState, id, output.ReqState);
                    StateManager.RequestedStates.Remove("os" + output.Id);
                }
                else if (reqState != output.State)
                {
                    StateManager.RequestedStates.Add("os" + output.Id, CurrentTicks);
                }
                else if (reqState == output.State)
                {
                    _logger.Warn(
                        "While setting ReqState to {0}: output {1} is already in that state.",
                        reqState, id);
                }
                output.ReqState = reqState;
                StateManager.SetObjectStateChanged(id, TLCObjectType.Output);
            }
            else
            {
                _logger.Error("SetOutputReqState: id {0} not found in StateManager instance.", id);
                throw new TLCObjectNotFoundException($"Id {id} not found in StateManager instance.");
            }
        }

        /// <summary>
        /// Retrieves the actual value of the State property of the signalgroup with the given id
        /// Throws TLCObjectNotFoundException if the id is not found
        /// </summary>
        public SignalGroupState? GetSignalGroupState(string id)
        {
            var signalGroup = StateManager.InternalSignalGroups.First(x => x.Id == id);
            if (signalGroup != null)
            {
                return signalGroup.State;
            }
            else
            {
                _logger.Warn("GetSignalGroupReqState: id {0} not found in StateManager instance.", id);
                throw new TLCObjectNotFoundException($"Id {id} not found in StateManager instance.");
            }
        }

        /// <summary>
        /// Retrieves the actual value of the State property of the output with the given id
        /// Throws TLCObjectNotFoundException if the id is not found
        /// </summary>
        public int? GetOutputState(string id)
        {
            var output = StateManager.InternalOutputs.First(x => x.Id == id);
            if (output != null)
            {
                return output.State;
            }
            else
            {
                _logger.Warn("GetOutputState: id {0} not found in StateManager instance.", id);
                throw new TLCObjectNotFoundException($"Id {id} not found in StateManager instance.");
            }
        }
        /// <summary>
        /// Retrieves the actual value of the State property of the detector with the given id
        /// Throws TLCObjectNotFoundException if the id is not found
        /// </summary>
        public DetectorState? GetDetectorState(string id)
        {
            var detector = StateManager.InternalDetectors.First(x => x.Id == id);
            if (detector != null)
            {
                return detector.State;
            }
            else
            {
                _logger.Warn("GetDetectorState: id {0} not found in StateManager instance.", id);
                throw new TLCObjectNotFoundException($"Id {id} not found in StateManager instance.");
            }
        }
        
        /// <summary>
        /// Retrieves the actual value of the State property of the input with the given id
        /// Throws TLCObjectNotFoundException if the id is not found
        /// </summary>
        public int? GetIntputState(string id)
        {
            var input = StateManager.InternalInputs.First(x => x.Id == id);
            if (input != null)
            {
                return input.State;
            }
            else
            {
                _logger.Warn("GetIntputState: id {0} not found in StateManager instance.", id);
                throw new TLCObjectNotFoundException($"Id {id} not found in StateManager instance.");
            }
        }

        public IntersectionControlState? GetIntersectionControlState(string id)
        {
            var intersection = StateManager.InternalIntersections.First(x => x.Id == id);
            if (intersection != null)
            {
                return intersection.State;
            }
            else
            {
                _logger.Warn("GetDetectorState: id {0} not found in StateManager instance.", id);
                throw new TLCObjectNotFoundException($"Id {id} not found in StateManager instance.");
            }
        }

        #endregion // Public Methods

        #region Private Methods

        private async void OnSessionControlChanged(object sender, ControlStateChangedEventArgs state)
        {
            switch (state.NewState)
            {
                case ControlState.StartControl:
                    _logger.Info("Received signal to take control from TLC");
                    StartControlRequestReceived?.Invoke(this, EventArgs.Empty);
                    break;
                case ControlState.EndControl:
                    _logger.Info("Received signal to release control from TLC. Leaving a maximum of 180 seconds to release control.");
                    try
                    {
                        if (state.OldState == ControlState.InControl &&  _session.State.IntersectionControl)
                        {
                            _controlEndingCancellationTokenSource = new CancellationTokenSource();
                            EndControlRequestReceived?.Invoke(this, EventArgs.Empty);
                            var start = DateTime.Now;
                            await Task.Run(async () =>
                            {
                                try
                                {
                                    await Task.Delay(180000, _controlEndingCancellationTokenSource.Token);
                                    _logger.Error("Control was not released after 180 seconds; will force ending control.");
                                }
                                catch (TaskCanceledException)
                                {
                                    _logger.Info("Control was released after {0} seconds.", DateTime.Now.Subtract(start).TotalSeconds);
                                }
                            }, _mainCancellationToken);
                            if (_userWantsControl && StateManager.ControlSession.ReqControlState != ControlState.ReadyToControl)
                            {
                                await _session?.SetReqControlStateAsync(ControlState.ReadyToControl);
                            }
                            else if (StateManager.ControlSession.ReqControlState != ControlState.Offline)
                            {
                                await _session?.SetReqControlStateAsync(ControlState.Offline);
                            }
                            LostControl?.Invoke(this, true);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _controlEndingCancellationTokenSource.Cancel();
                    }
                    break;
                case ControlState.InControl:
                    GotControl?.Invoke(this, EventArgs.Empty);
                    if (StateManager.Intersection.State != null &&
                        StateManager.Intersection.State.Value != IntersectionControlState.Control &&
                        (StateManager.Intersection.ReqState == null ||
                         _userRequestedIntersectionControlState != StateManager.Intersection.State.Value &&
                         _userRequestedIntersectionControlState != StateManager.Intersection.ReqState.Value))
                    {
                        await _session.SetIntersectionReqStateAsync(_userRequestedIntersectionControlState);
                    }
                    break;
                case ControlState.Offline:
                    if (_userWantsControl &&
                        StateManager.ControlSession.ControlState != null &&
                        StateManager.ControlSession.ControlState.Value == ControlState.Offline)
                    {
                        await _session.SetReqControlStateAsync(ControlState.ReadyToControl);
                    }
                    break;
                case ControlState.Error:
                case ControlState.NotConfigured:
                case ControlState.ReadyToControl:
                case null:
                    if (StateManager.ControlSession.ReqControlState != null && 
                        StateManager.ControlSession.ReqControlState.Value == ControlState.InControl)
                    {
                        LostControl?.Invoke(this, false);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void OnSessionEnded(object sender, bool expected)
        {
            SessionEnded?.Invoke(this, expected);
        }

        private void OnIntersectionControlChanged(object sender, IntersectionControlState? state)
        {
            if(state.HasValue) IntersectionStateChanged?.Invoke(this, state.Value);
            switch (state)
            {
                case IntersectionControlState.Error:
                    break;
                case IntersectionControlState.Dark:
                    break;
                case IntersectionControlState.Standby:
                    break;
                case IntersectionControlState.AlternativeStandby:
                    break;
                case IntersectionControlState.SwitchOn:
                    break;
                case IntersectionControlState.SwitchOff:
                    break;
                case IntersectionControlState.AllRed:
                    break;
                case IntersectionControlState.Control:
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private async void StateManager_StateChanged(object sender, List<Tuple<string, TLCObjectType>> ids)
        {
            if (_settingState)
            {
                _logger.Warn("StateManager_StateChanged was called while a previous call was still being handled.");
                return;
            }
            if (ids.Count == 0)
            {
                return;
            }
            _settingState = true;

            // Build a dictionary with a list of changed objects per object type
            var update = new Dictionary<TLCObjectType, List<TLCObjectBase>>();
            foreach (var id in ids)
            {

                var obj = StateManager.FindObjectById(id.Item1, TLCFIClientStateManager.GetObjectTypeString(id.Item2));
                var obt = ((TLCObjectBase)obj).ObjectType;

                var upo = (TLCObjectBase)obj;
                if (!update.ContainsKey(obt))
                {
                    update.Add(obt, new List<TLCObjectBase> { upo });
                }
                else
                {
                    update[obt].Add(upo);
                }
            }

            // Send the update with all data
            await _session.TLCProxy.UpdateStateAsync(new ObjectStateUpdateGroup
            {
                Ticks = CurrentTicks,
                Update = update.Select(t => new ObjectStateUpdate()
                    {
                        Objects = new ObjectReference()
                        {
                            Ids = t.Value.Select(x => x.Id).ToArray(),
                            Type = t.Key
                        },
                        States = t.Value.Select(x => x.GetState()).ToArray()
                    })
                    .ToArray()
            }, _mainCancellationToken);

            _settingState = false;
        }

        #endregion // Private Methods

        #region Constructor

        public TLCFIClient(TLCFIClientConfig config, CancellationToken token)
        {
            _config = config;
            _mainCancellationToken = token;
            token.Register(() => { _sessionCancellationTokenSource.Cancel(); });
            var endPoint = new IPEndPoint(IPAddress.Parse(_config.RemoteAddress), config.RemotePort);
            _sessionManager = new TLCFIClientSessionManager(endPoint);
            _sessionManager.TLCSessionEnded += OnSessionEnded;
            _clientInitializer = new TLCFIClientInitializer(_config);
        }

        #endregion // Constructor
    }
}
