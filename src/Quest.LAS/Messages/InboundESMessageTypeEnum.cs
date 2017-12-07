namespace Quest.LAS.Messages
{
    public enum InboundESMessageTypeEnum
    {
        Z03, //alternate nsclient
        Z04, //show engineering screen
        Z05, //acceptance of auto avls
        Z06, //show reboot button
        Z07, //request conf params from CAD
        Z09, //force mdt shutdown
        Z13, //execute dos command
        Z14, //switch to alternate db
        Z99 //mdtes eng data has been received
    }

}
