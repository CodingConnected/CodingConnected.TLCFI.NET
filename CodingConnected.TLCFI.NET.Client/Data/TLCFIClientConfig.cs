using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CodingConnected.TLCFI.NET.Models.Generic;
using CodingConnected.TLCFI.NET.Models.TLC;
using JetBrains.Annotations;

namespace CodingConnected.TLCFI.NET.Client.Data
{
    /// <summary>
    /// Configuration used by instances of TLCFIClient
    /// </summary>
    public class TLCFIClientConfig
    {
        #region Properties

        /// <summary>
        /// Remote address of the TLC Facilities to connect to
        /// </summary>
        public string RemoteAddress { get; [UsedImplicitly] set; }

        /// <summary>
        /// Remote port where the TLC Facilities exposes the TLC-FI
        /// </summary>
        public int RemotePort { get; [UsedImplicitly] set; }

        /// <summary>
        /// Username to use for registering with the TLC Facilities
        /// </summary>
        public string Username { get; [UsedImplicitly] set; }

        /// <summary>
        /// Password to use for registering with the TLC Facilities
        /// </summary>
        public string Password { get; [UsedImplicitly] set; }

        /// <summary>
        /// URI where the TLC Facilities can find the IVERA server of the CLA
        /// Note: this value is obligatory and should also be set to a valid URI if no IVERA server is present
        /// </summary>
        public string IveraUri { get; [UsedImplicitly] set; }

        /// <summary>
        /// The type of application; this will be checked with that configured remotely
        /// </summary>
        public ApplicationType ApplicationType { get; [UsedImplicitly] set; }

        /// <summary>
        /// The id of the intersection to monitor and/or control via the TLC-FI
        /// </summary>
        public string RemoteIntersectionId { get; [UsedImplicitly] set; }

        /// <summary>
        /// Start capability of this CLA (presupposes ApplicationType to be Control)
        /// </summary>
        public HandoverCapability StartCapability { get; [UsedImplicitly] set; }

        /// <summary>
        /// End capability of this CLA (presupposes ApplicationType to be Control)
        /// </summary>
        public HandoverCapability EndCapability { get; [UsedImplicitly] set; }

        /// <summary>
        /// Time to wait after succesfully registering, before continuing with configuration, subscription, etc.
        /// This allows a remote TLC Facilities to execute time-consuming tasks after the registration
        /// </summary>
        public int RegisterDelayAfterConnecting { get; [UsedImplicitly] set; }

        /// <summary>
        /// Use ids from the TLC instead of the intersection for reading META data and subscribing
        /// </summary>
        public bool UseIdsFromTLCForSubscription { get; [UsedImplicitly] set; }

        /// <summary>
        /// Subscribe to all outputs, not only the exclusive one connected to configured intersection
        /// Note: this has no effect if UseIdsFromTLCForSubscription is set to true
        /// </summary>
        public bool SubscribeToAllOutputs { get; [UsedImplicitly] set; }

        /// <summary>
        /// Automatically reconnect upon connection loss
        /// Note: after a fatal error occured, reconnection will not occur regardless of this setting
        /// </summary>
        public bool AutoReconnect { get; [UsedImplicitly] set; }

        /// <summary>
        /// Maximum number of times the TLC Facilities can set the ControlState for the session to Error,
        /// before this is considered fatal
        /// </summary>
        public int MaxSessionErrorCount { get; [UsedImplicitly] set; }

        /// <summary>
        /// List of ids of signalgroups that are expected to be found in the TLC Facilities
        /// Note: this list must exactly match that in the TLC
        /// </summary>
        [XmlArrayItem(ElementName = "Id")]
        public List<string> SignalGroupIds { get; set; }

        /// <summary>
        /// List of ids of detectors that are expected to be found in the TLC Facilities
        /// Note: all detectors in this list must be present in the TLC; if detectors from the TLC are not in
        /// the list, a warning will be issued
        /// </summary>
        [XmlArrayItem(ElementName = "Id")]
        public List<string> DetectorIds { get; set; }

        /// <summary>
        /// List of ids of inputs that are expected to be found in the TLC Facilities
        /// Note: all inputs in this list must be present in the TLC; if inputs from the TLC are not in
        /// the list, a warning will be issued
        /// </summary>
        [XmlArrayItem(ElementName = "Id")]
        public List<string> InputIds { get; set; }

        /// <summary>
        /// List of ids of outputs that are expected to be found in the TLC Facilities
        /// Note: all outputs in this list must be present in the TLC; if outputs from the TLC are not in
        /// the list, a warning will be issued
        /// </summary>
        [XmlArrayItem(ElementName = "Id")]
        public List<string> OutputIds { get; set; }

        #endregion // Properties

        #region Constructor

        public TLCFIClientConfig()
        {
            SignalGroupIds = new List<string>();
            DetectorIds = new List<string>();
            OutputIds = new List<string>();
            InputIds = new List<string>();
        }

        #endregion // Constructor
    }
}