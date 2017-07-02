using Quest.Common.Messages;

namespace Quest.Lib.Device
{
    public interface IDeviceStore
    {
        QuestDevice Get(string deviceIdentity);
        QuestDevice GetByToken(string token);
        void Update(QuestDevice device);
    }
}