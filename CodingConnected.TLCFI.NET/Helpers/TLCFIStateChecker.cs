using System;
using CodingConnected.TLCFI.NET.Models.TLC;
using NLog;

namespace CodingConnected.TLCFI.NET.Helpers
{
    public static class TLCFIStateChecker
    {
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
