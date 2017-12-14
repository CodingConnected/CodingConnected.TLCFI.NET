using System;
using CodingConnected.TLCFI.NET.Core.Models.TLC;
using CodingConnected.TLCFI.NET.Core.Models.TLC;

namespace CodingConnected.TLCFI.NET.Core.Helpers
{
    public class ControlStateChangedEventArgs : EventArgs
    {
        public ControlState? OldState { get; }
        public ControlState? NewState { get; }

        public ControlStateChangedEventArgs(ControlState? newState, ControlState? oldState)
        {
            NewState = newState;
            OldState = oldState;
        }
    }
}
