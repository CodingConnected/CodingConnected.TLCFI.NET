# TLC-FI.NET

- [TLC-FI.NET](#tlc-finet)
  * [Overview](#overview)
    + [Building the libary](#building-the-libary)
    + [Compatibility](#compatibility)
  * [Using the API: the TLCFIClient class](#using-the-api--the-tlcficlient-class)
    + [Use case: using TLCFIClient in a CLA](#use-case--using-tlcficlient-in-a-cla)
    + [Constructor](#constructor)
    + [Events](#events)
    + [Functions](#functions)
  * [Other classes](#other-classes)

## Overview

TLCFI.NET is a full client-side implementation of the Traffic Light Controller Facilities Interface for the .NET family of languages. It is provides a convenient way to connect a client application (CLA) to a traffic light controller (TLC). After proper configuration, the library takes care of opening and maintaining the connection with the TLC, logging in, and monitoring state variables and/or setting state request variables in the connected TLC.

To properly facilitate common development scenarios in the field of traffic management, TLCFI.NET has been designed mostly asynchronously and event-driven. This way, the API can be consumed without blocking any other tasks an application might have, such as interfacing with other systems, calculating the most efficient way to regulate traffic, etc.

This documentation assumes working knowledge of the TLC-FI and Generic-FI IDD's. It is thus assumed the reader and potential user of the API, is familiar with terms such as TLC and CLA, the roles of TLC and CLA, as well as the supposed chain of commands and events that may occur during a session between an CLA and a TLC.

### Building the libary
The Visual solution uses Paket at its dependency manager. Therefor, in order to build the project, first retrieve the dependencies as follows:
- Open the Packet Manager Console via menu Tools > Nuget Packet Manager.
- At the console prompt, type: .paket\paket.bootstrapper.exe
 - The bootstrapper now downloads the latest version of Paket
- At the console prompt, type: .paket\paket.exe restore
 - This will restore all dependencies (NLog)

### Compatibility

The library target the .NET Framework version 4.5 and is developed using Visual Studio 2017. It has also been tested with various versions of Mono. The current version works well with Mono, though it cannot handle loss of connection yet, which raises an exception not raised with .NET. To use the libary with Mono, it is easiest to build a binary on Windows, and subsequently use that with Mono.

## Using the API: the TLCFIClient class

The API of TLC-FI.NET currently consists mainly of one class: TLCFIClient. Other public classes in the library are either meant for use with TLCFICLient (such as the TLCFIClientConfig class in the TLCFI.NET.Client.Data namespace), or allow more low-level interaction with the capcbilities of the libary. The TLCFIClient class exposes functions to connect and configure the client application, and read state from and set requests in the TLC. It also exposes a number of .NET events that may be used react to events occuring in the TLC or in the connection with the TLC.

Typically, the functions and events that the TLCFIClient class exposes will be used together, as follows:
- A host application uses the functions to initiate actions like connecting, requesting control, and setting state.
- Most functions will (immediately or at some point later in time) cause a response from the TLC, which will trigger and event. A host application react to these events by taking appropriate action, and/or calling a further function on the client.

### Use case: using TLCFIClient in a CLA

As a pratical example, here is typical use case, assuming the CLA will take control:
- The host application creates an instance of `TLCFIClient`
- `StartSessionAsync()` is called to start a session
- The moment a session has succesfully been started, the `ClientInitialized` event will be raised. As a response, the host application calls `RequestSessionStartControl()`.
- The moment the TLC reqeusts the CLA to take session control, the ` StartControlRequestReceived` event is raised (this is mostly informative, see below). The TLCFIClient will automatically confirm the request to take control, confirming it is now in control. Once the TLC acknowledges that confirmation, the `GotControl` event is raised.
- From here on, the host application is in control of the intersection, and may set intersection, signal group and output states. The host application is responsible for setting the appropriate (ie. confirming to all minimum times and conflicts) states via calls to `RequestIntersectionState()` and `UpdateState()`.
- If the TLC revokes control, the `EndControlRequestReceived` event will be raised. The host application must now release control within the prescribed timeout. The host application can confirm release of control using `ConfirmEndControl()`. It may happen that the TLC revokes control before then, depending on the setting of EndCapability on the client and the state of the intersection.
- The host application can also take the initiave to end control, by calling `RequestSessionEndControl()`. This should result in a confirmation from the TLC, which will cause the `EndControlRequestReceived` event to be raised, as described at the previous bullet.
- Once the TLC revokes control, the `LostControl` event will be raised, meaning the session is no longer in control, and may no longer set (exclusive) states.

### Constructor

The constructor of the `TLCFIClient` class takes two arguments:
- An instance of `TLCFIClientConfig`: this class contains all relevant configuration data. The client will use this data to initiate the connection with the TLC. A concise list of members of this class:
 - `FiVersion` A static instance of ProtocolVersion, describing which version of the TLC-FI the library implements. Currently: 1.1.0.
 - `RemoteAddress`, `RemotePort`, `Username`, `Password` Remote address, port, and username and password, used to connect and login.
 - `ApplicationType` Application type (consumer, provider or control)
 - `RemoteIntersectionId` The remote id of the intersection that will be monitored and/or controlled
 - `StartCapability`, `EndCapability` Start- and capability of the host application
 - `IveraUri` A URI describing where the CLA IVERA client can be accessed
 - `UseIdsFromTLC` A boolean which, if true, causes the client to check ids with TLC id's, instead of intersection id's.
 - `RegisterDelayAfterConnecting` Time, in miliseconds, to wait before registering after a TCP connection has been established.
 - `AutoReconnected` A boolean which, if true, causes the client to automatically reconnect upon connection loss.
 - `SignalGroupIds` A list of strings, holding all the signal group id's that are expected to be found in the TLC configuration.
 - `DetectorIds`, `InputIds` and `OutputIds` Same as previous bullet, for other objects. Note that for signal groups, the list of ids must match exactly. For the other objects, an id present in the TLC that is not present in the local list will cause a warning; an id present locally but not in the TLC will cause an error.
- An instance of a `CancellationToken`. If `Cancel()` is called on the source of this token, all tasks running within the client will abruptly ended, ending a running session non-gracefully.

### Events

Below is a more detailed description of the events that are exposed by the TLCFIClient class.
- `ClientInitialized` Raised as a result of a call to `StartSessionAsync()` once the connection with the TLC is established and initialized.
- `StartControlRequestReceived` Raised when a StartControl message is sent by the TLC. This normally happens as a result of calling RequestSessionStartControl (see below). The TLCFIClient class will response immediatelly by confirming control, after which the GotControl event should be raised. This event is thus mostly informative. It might be used in the future, to allow for actions to be taken after the start control request, before actually taking control.
- `EndControlRequestReceived` Raised when the TLC requests the CLA to release control. This can happen cause the TLC want to hand over control to another CLA, or because the CLA requested it by calling `RequestSessionEndControl()`
- `GotControl` Raised once the CLA actually got control. This means the *session* got control, and can now request state changes, on the intersection, signalgroups and (exclusive) outputs. Note that only once the *intersection* is also in the control state, may state changes be requested for signalgroups and outputs.
- `LostControl` Raised once the CLA definitively lost control. There is a period of time, which has a prescribed maximum, in between the `EndControlRequestReceived` event and the `LostControl`, where the CLA is allowed to take actions before releasing control. `ConfirmEndControl()` can be called to end this period. Note that the TLC may actually revoke control earlier than a call to `ConfirmEndControl()`, depending on the configured EndCapability in combination with the state of the intersection.
- `IntersectionStateChanged` Raised whenever the intersection that has been subscribed to changes its state.

### Functions

Alongside events, the TLCFIClient class exposes a number of functions, meant to send requests to the TLC. All functions are implemented asynchronously. These functions are:
- `StartSessionAsync()` Tries to start a TCP session with the TLC. Once connected, the function will take care of logging in, retrieving all objects from the TLC for the configured intersection, check the data against the local configuration, and set session state to configured. Note: this function will never actually return, cause it contains logic to rebuild lost connections, unless instructed so by a call to `EndSessionAsync()`. When the function succesfully initiates a session, the `ClientInitialized` event is raised.
- `RequestSessionStartControl()` will cause a request to be sent to the TLC to hand over control to the CLA: ReadyToControl. This should be called after a succesful call to `StartSessionAsync()`, typically as a response to the `ClientInitialized` event. It may take any amount of time for the TLC to grant this request, based on settings within the TLC. The TLCFIClient will wait indefinitely in a ReadyToControl state for this to happen.
- `RequestSessionEndControl()` this should only be called while the session has control, otherwise a warning will be issued. It will cause an EndControl message to be sent to the TLC, indicating the CLA wants to release control. The TLC will respond immediatally by confirming EndControl, which will cause the EndControlRequestReceived event to be triggered. The CLA should act accordingly as a response to this event to facilitate ending control, before calling `ConfirmEndControl()`.
- `RequestIntersectionState()` This should only be called while the session has control. The function forwards a request to set the state for the intersection the CLA is in control of. It may therefor only be called by applications of type ControlApplication. For example, this function is needed if StartCapability is Cleared, and the intersection is handed in an AllRed state. Or, it may be used to darken an intersection in a given (larger scale) scenario.
- `ConfirmEndControl()` This function should be called once ending control is finished, and control released. It will cause the session state to be set to ReadyToControl in case the control was ended by the TLC while the CLA still wants control. Otherwise, if control was released by a call to RequestSessionEndControl, the session state will be set to Offline. As noted above the TLC may already have revoked control before this function is called, depending on the setting of EndCapability in combination with the state of the intersection.
- `UpdateState` This will cause states that were changed since the last call to UpdateState to be synchronized with the TLC, or do nothing if nothing changed. Note: this function is not asynchronous, but the backend does handle synchronising state asynchronously, and the function will never stall. It will typically be called in a loop
- `EndSessionAsync` this causes the session to end, and the reconnect loop to be broken. The session will be ended gracefully if possible, by releasing control, and logging out if applicable.

## Other classes
The library has other public classes aside from TLCFIClient and TLCFIClientConfig. The public intrafcing of these classes is still in development. This readme will be filled with further details as the API for those classes becomes more stable.