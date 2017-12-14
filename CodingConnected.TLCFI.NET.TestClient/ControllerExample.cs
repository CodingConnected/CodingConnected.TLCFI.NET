using NLog;
using System;
using System.Linq;
using System.Threading;
using CodingConnected.TLCFI.NET.Client;
using CodingConnected.TLCFI.NET.Core.Models.TLC;

namespace CodingConnected.TLCFI.NET.Core.Core.TestClient
{
    /// <summary>
    /// An example of a class that consumes the events and methods of TLCFIClient.
    /// Typically, a controller loop will run wihtin a class such as this, which will
    /// use state data from the TLC Facilities as input, and set requested state via
    /// the TLCFIClient in the TLC Facilities.
    /// <remarks>NOTE: In this example, not all event handlers have been implemented async.
    /// It is good practice to implement using async where possible, cause the event
    /// handlers should not stall.</remarks>
    /// </summary>
    public class ControllerExample
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static TLCFIClient _fiClient;

        #endregion // Fields

        #region Properties

        #endregion // Properties

        #region Events

        #endregion // Events
        
        #region Public Methods

        public void TakeStep()
        {
            // Execute controller algorithms to determine desired state for signal groups and 
            // outputs (and potentially the intersection itself).
            // Subsequently, use calls like this
            //     _fiClient.SetSignalGroupReqState("sg1", SignalGroupState.StopAndRemain);
            //     _fiClient.SetOutputReqState("out1", 1);
            // to request remote state.
            // Requests for remote state then need to be forwarded to the TLC by calling UpdateState():
            _fiClient.UpdateState();
        }

        #endregion // Public Methods

        #region Private Methods

        private void OnDetectorStateChanged(object sender, Detector detector)
        {
            // Handle detector state change; for example, set detector state in controller
            // Alternatively, one could use the Get... methods of TLCFIClient to get detector,
            // signal group
        }

        private void OnStartControlRequestReceived(object sender, EventArgs e)
        {
            // Handle request to start control; for example, bring controller in ready state
        }

        private async void OnEndControlRequestReceived(object sender, EventArgs e)
        {
            // Handle request to end control; for example, end all green phases
            // Then, confirm that control has ended
            await _fiClient.ConfirmEndControl(CancellationToken.None);
        }

        private async void OnGotControl(object sender, EventArgs e)
        {
            // Handle getting control; from this moment onwards, the session is in control and
            // may set ReqState for various objects

            // Supposing we know the id of the intersection we will control, we can check its
            // control state:
            //     var intersectionState = _fiClient.GetIntersectionControlState("interId");
            // and set it to control if needed:
            //     if (IntersectionState != IntersectionControlState.Control)
            //     {
            //         _fiClient.SetIntersectionReqControlState(IntersectionControlState.Control);
            //     }

            // Or, if we don't know the id, we can use the statemanager for more direct access
            // Note: use the state manager with care when setting state. Using the public Set 
            // methods on TLCFIClient is the prefered way to set requested state.
            var intersection = _fiClient.StateManager.Intersections.FirstOrDefault();
            if (intersection != null)
            {
                if (!intersection.State.HasValue ||
                    intersection.State.Value != IntersectionControlState.Control)
                {
                    await _fiClient.SetIntersectionReqControlState(IntersectionControlState.Control);
                }
            }
        }

        private void OnLostControl(object sender, bool expected)
        {
            // Handle loss of control; from this moment onwards, ReqState may no longer be set
        }

        private void OnIntersectionStateChanged(object sender, IntersectionControlState state)
        {
            // Handle intersection state change; for example, start controller algorithms if
            // state is set to Control
            // Note: no ReqState may be set for signal groups and exclusive outputs while the
            // intersection is not in Control state
        }

        #endregion // Private Methods

        #region Constructor

        public ControllerExample(TLCFIClient fiClient)
        {
            _fiClient = fiClient;

            fiClient.DetectorStateChanged += OnDetectorStateChanged;
            fiClient.StartControlRequestReceived += OnStartControlRequestReceived;
            fiClient.EndControlRequestReceived += OnEndControlRequestReceived;
            fiClient.GotControl += OnGotControl;
            fiClient.LostControl += OnLostControl;
            fiClient.IntersectionStateChanged += OnIntersectionStateChanged;
            // There are more events, not used in this example:
            //     fiClient.ClientInitialized += OnClientInitialized;
            //     fiClient.SignalGroupStateChanged += OnSignalGroupStateChanged;
            //     fiClient.InputStateChanged += OnInputStateChanged;
            //     fiClient.OutputStateChanged += OnOutputStateChanged;
        }

        #endregion // Constructor
    }
}