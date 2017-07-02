namespace Quest.Lib.EISEC
{
    public enum ReturnCode
    {
        Success,
        Exception,
        Timeout,
        CommunicationFailure,
        LogonRejected,
        LogoffFailed,
        PasswordRejected,
        QueryFailed,
        UnknownPdu,
        NotLoggedIn,
        InvalidUserId,
        IpListEmpty,
        Unsupported
    }
}