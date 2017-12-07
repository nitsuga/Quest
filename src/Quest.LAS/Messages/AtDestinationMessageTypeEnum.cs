using System.Collections.Generic;
using System.Text;

namespace Quest.LAS.Messages
{

    public enum AtDestinationMessageTypeEnum
    {
        AtDestinationNone = 0,
        AtIncidentNaviation = 1,
        AtIncidentIgnitionOffDistance = 2,
        AtIncidentStationaryTimeDistance = 3,
        AtIncidentIgnitionOff = 4,
        AtHospitalNavigation = 5,
        AtDestinationNavigation = 6,
        EGreenStation = 7,
        EAmberMobile = 8,
        ERedAtScene = 9,
        ERedAtHospital = 10,
        MdtLogIgnitionOn = 11,
        MdtLogHbOff = 12,
        MdtLogHbOn = 13,
        MdtLogMovingStarted = 14,
        MdtLogMovingStopped = 15,
        MdtLogBlueLightStarted = 16,
        MdtLogBlueLightStopped = 17,
        MdtLogTwoToneStarted = 18,
        MdtLogTwoToneStopped = 19,
        NumRadiosZero = 40,
        NumRadiosOne = 41,
        NumRadiosTwo = 42
    }

}
