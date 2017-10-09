using Quest.Common.Messages;
using System;

namespace Quest.Lib.Device
{
    public interface IDeviceStore
    {
        QuestDevice Get(string deviceIdentity);
        QuestDevice GetByToken(string token);
        QuestDevice Update(QuestDevice device, DateTime timestamp);
    }
}