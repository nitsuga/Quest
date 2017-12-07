namespace Quest.LAS.Messages
{
    public class SetSkillLevel : IDeviceMessage
    {
        public int? StatusEasting;
        public int? StatusNorthing;
        public char SkillLevel;
    }

}
