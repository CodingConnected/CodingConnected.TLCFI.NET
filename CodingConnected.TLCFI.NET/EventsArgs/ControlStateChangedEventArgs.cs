using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Models.TLC;

namespace CodingConnected.TLCFI.NET.EventsArgs
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
