namespace Quest.LAS.Messages
{
    public enum CadStatusOrigin
    {
        R = 0x52, //Airwave via DIBA
        B = 0x42, //CAD status acceptance bounce
        J = 0x4A, //Initiated by a CAD automated process/job
        U = 0x55 //Initiated by a user action
    }

}
