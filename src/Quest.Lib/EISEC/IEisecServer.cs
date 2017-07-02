using Quest.Common.Messages;

namespace Quest.Lib.EISEC
{
    public interface IEisecServer
    {
        void UpdatePassword(int _index, string _pendingPasswordChange);
        void SetAddress(int reqNo, CallLookupResponse details);
    }
}
