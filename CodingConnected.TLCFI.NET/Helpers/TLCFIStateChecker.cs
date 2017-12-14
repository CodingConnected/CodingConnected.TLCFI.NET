using System;
using CodingConnected.TLCFI.NET.Core.Models.TLC;

namespace CodingConnected.TLCFI.NET.Core.Helpers
{
	/// <summary>
	/// This class is meant for checking if control state changes are valid
	/// </summary>
    public static class TLCFIStateChecker
    {
		/// <summary>
		/// Check wether a given change in the control state of a TLC is valid.
		/// The result of this method can be used to report a faulty state change,
		/// and/or take appropriate action.
		/// </summary>
		/// <param name="oldstate">The prievous control state</param>
		/// <param name="newstate">The new control state</param>
		/// <returns>True if a state change is OK, false if not</returns>
        public static bool IsControlStateChangeOk(ControlState oldstate, ControlState newstate)
        {
            if (newstate == ControlState.Error)
            {
                return true;
            }
            switch (oldstate)
            {
                case ControlState.Error:
                    if (newstate == ControlState.NotConfigured) return true;
                    return false;
                case ControlState.NotConfigured:
                    if (newstate == ControlState.Offline) return true;
                    return false;
                case ControlState.Offline:
                    if (newstate == ControlState.ReadyToControl) return true;
                    return false;
                case ControlState.ReadyToControl:
                    if(newstate == ControlState.StartControl) return true;
                    return false;
                case ControlState.StartControl:
                    if (newstate == ControlState.InControl ||
                        newstate == ControlState.Offline) return true;
                    return false;
                case ControlState.InControl:
                    if (newstate == ControlState.EndControl ||
                        newstate == ControlState.ReadyToControl ||
                        newstate == ControlState.Offline) return true;
                    return false;
                case ControlState.EndControl:
                    if (newstate == ControlState.ReadyToControl ||
                        newstate == ControlState.Offline) return true;
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(oldstate), oldstate, null);
            }
        }
    }
}
