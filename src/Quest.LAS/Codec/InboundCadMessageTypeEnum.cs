namespace Quest.LAS.Codec
{
    public enum InboundCadMessageTypeEnum
    {
        MIT_CALLSIGN_PARAM_CALLSIGN = 0x21,
        MIT_CALLSIGN_PARAM_STATUS = 0x2A,
        MIT_CMD_INCIDENT = 0x49,
        MIT_CALLSIGN_PARAM_INCIDENT = 0x2B,
        InboundMessageIdentifier = 0x53,
        MIT_CMD_SEND_MESSAGE = 0x4D,
        MIT_MSG_PRIORITY = 0x1F,
        MIT_MSG_TEXT = 0x20,
        MIT_CMD_GENERAL_BROADCAST = 0x47,
        MIT_CMD_PARAMETERS = 0x50,
        MIT_CMD_CANCEL_INCIDENT = 0x58,
        MIT_CMD_ADMIN = 0x41,
        MIT_CMD_SEND_STATUS_R = 0x52,
        MIT_CMD_SEND_STATUS_B = 0x42,
        MIT_CMD_SEND_STATUS_J = 0x4A,
        MIT_CMD_SEND_STATUS_U = 0x55,
        EngineeringMessage = 0x5A
    }
}
