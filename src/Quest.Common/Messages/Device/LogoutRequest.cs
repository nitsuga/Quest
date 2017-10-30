﻿using System;

namespace Quest.Common.Messages.Device
{
    
    /// <summary>
    ///     A device requests that it is to be deregistered for activity. No more push messages
    ///     will be sent to the device and the AuthToken will be unauthorised. Subsequent messages
    ///     that contain the devices AuthToken will be responded to with an authorisation failure
    /// </summary>
    [Serializable]
    public class LogoutRequest : Request
    {
    }
        
}