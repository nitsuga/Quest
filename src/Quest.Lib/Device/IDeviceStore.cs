using Quest.Common.Messages;
using Quest.Common.Messages.Device;
using System;
using System.Collections.Generic;

namespace Quest.Lib.Device
{
    public interface IDeviceStore
    {
        QuestDevice Get(string deviceIdentity);
        QuestDevice GetByToken(string token);
        List<QuestDevice> GetByFleet(string fleetNo);
        QuestDevice Update(QuestDevice device, DateTime timestamp);
    }
}