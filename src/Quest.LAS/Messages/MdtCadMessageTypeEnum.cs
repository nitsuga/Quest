namespace Quest.LAS.Messages
{
    public enum MdtCadMessageTypeEnum
    {
        Environment = 0,
        SatNav = 10,
        GeneralBroadcast = 30,
        TextMessage = 40,
        NewEventUpdate = 50,
        ElectronicallyDespatched = 52,
        EventExchange = 53,
        Cancellation = 55,
        SetSkillLevel = 60,
        StatusChange = 70,
        CallStillInProgress = 72,
        SetNumberOfRadios = 80,
        PossibleHighRisk = 90,
        Aos = 100,
        NotSet = 999,
        StickyMessage = 1000
    }

}
