using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using www.aspect.com.unifiedip.edk.clientpublishapi._2009._08;
using www.aspect.com.unifiedip.edk.commondata._2009._08;

namespace Quest.Lib.Telephony.AspectCTIPS
{
    public class CTIEventHandler : CTIEventService
    {
        public event System.EventHandler HeartbeatEvent;

        private string EventCategory;
        private string EventType;
        private string requestId;
        private string _channelName;
        
        /// <summary>
        /// chanell used for talking to the workflow
        /// </summary>
        private ICADChannel _cadChannel;

        /// <summary>
        /// A list of agents and the station address they arre using.
        /// </summary>
        private Dictionary<int, String> _agents = new Dictionary<int, String>();

        public CTIEventHandler(String channelName, ICADChannel cadChannel)
        {
            _cadChannel = cadChannel;
            _channelName = channelName;

        }

        private void MakeCall(int agent, string toExternalNumber)
        {
            
        }

        #region CTIEventService Members

        /// <summary>
        /// Print to the log
        /// </summary>
        /// <param name="printBuffer"></param>
        void PrintEvents(String printBuffer)
        {
            Logger.Write(string.Format("Channel {0} Event {1}", _channelName, printBuffer), TraceEventType.Information, "CTIPSChannel");
        }

        /// <summary>
        /// Receive events from the CTI server and send commands to the workflow as necessary
        /// </summary>
        /// <param name="ctiEvent"></param>
        /// <returns></returns>
        public int PublishCTIEvent(CTIEvent ctiEvent)
        {
            int res = 0;
            EventCategory = ctiEvent.EventCategory.ToString();
            EventType = ctiEvent.EventType.ToString();
            requestId = ctiEvent.RequestId.ToString();
            String printBuffer = "";

            switch (ctiEvent.EventType)
            {
                case CTIEventType.AgentLoginStateChange:
                    {
                        CTIAgentLoginStateChange alsc = ctiEvent as CTIAgentLoginStateChange;

                        printBuffer = "";

                        printBuffer += "Got AgentLoginStateChange Event:  LoginState: " + alsc.LoginState + "   AgentId: " + alsc.AgentId + "   RequestId: " + alsc.RequestId
                        + "\rStationId: " + alsc.AgentStationAddress + "   AgentWorkGrpId: " + alsc.AgentWorkGroupId + "   RsrcGrpId: " + alsc.ResourceGroupId
                        + "\rAgentLoginName: " + alsc.AgentLoginName + "   AgentCTIId: " + alsc.AgentCTIId + "   AgentMailbox: " + alsc.AgentMailbox + "   ServiceID: " + alsc.ServiceId
                        + "\rSiteId: " + alsc.SiteId + "   SwitchId: " + alsc.SwitchId + "   TenantId: " + alsc.TenantId + "   CircuitId: " + alsc.CircuitId + "   ChannelId: " + alsc.ChannelId;

                        if (alsc.ServiceState != null)
                        {
                            if (alsc.ServiceState.Length > 0)
                            {
                                for (int i = 0; i < alsc.ServiceState.Length; i++)
                                {
                                    if (i == 0 || i == 2 || i == 4 || i == 6 || i == 8 || i == 10 || i == 12)
                                    {
                                        printBuffer += "\r";
                                    }
                                    printBuffer += "ServiceID[" + i + "]: " + alsc.ServiceState[i].ServiceId + "   ServiceState[" + i + "]: " + alsc.ServiceState[i].ServiceState + "           ";
                                }
                            }
                        }

                        if (alsc.AgentCapabilities != null)
                        {
                            for (int i = 0; i < alsc.AgentCapabilities.Length; i++)
                            {
                                if (i == 0 || i == 2 || i == 4)
                                {
                                    printBuffer += "\r";
                                }
                                printBuffer += "AgentCapabilities[" + i + "]: " + alsc.AgentCapabilities[i] + "              ";
                            }
                        }
                        printBuffer += "\r-----------------------------------------------------------------------------------------------------------------------------\r";

                        PrintEvents(printBuffer);

                        if ( alsc.LoginState== CTIAgentLoginState.AgentLoggedIn)
                        {
                            Logger.Write(String.Format("Channel {0} Agent {1} {2} logged in on {3}", _channelName, alsc.AgentId, alsc.AgentLoginName, alsc.AgentStationAddress), TraceEventType.Information, "CTIPSChannel");
                            AddAgent(alsc.AgentStationAddress, alsc.AgentId);
                            _cadChannel.SendLogon(alsc.AgentStationAddress);
                        }

                        if ( alsc.LoginState== CTIAgentLoginState.AgentLoggedOut)
                        {                            
                            Logger.Write(String.Format("Channel {0} Agent {0} {1} logged out on {2}",_channelName, alsc.AgentId, alsc.AgentLoginName, alsc.AgentStationAddress), TraceEventType.Information, "CTIPSChannel");
                            RemoveAgent(alsc.AgentStationAddress, alsc.AgentId);
                            _cadChannel.SendLogoff(alsc.AgentStationAddress);
                        }
                        break;
                    }
                case CTIEventType.AgentStateChange:
                    {
                        CTIAgentStateChange asc = ctiEvent as CTIAgentStateChange;
                        
                        printBuffer = "";

                        printBuffer += "Got AgentStateChange Event:  AgentState: " + asc.AgentState
                            + "\rRequestId: " + asc.RequestId + "   SiteId: " + asc.SiteId + "   SwitchId: " + asc.SwitchId + "   TenantId: " + asc.TenantId
                            + "\rAgentId: " + asc.AgentId + "   ServiceId: " + asc.ServiceId + "   CallId: " + asc.CallId;

                        if (asc.ServiceIds != null)
                        {
                            if (asc.ServiceIds.Length > 0)
                            {
                                for (int i = 0; i < asc.ServiceIds.Length; i++)
                                {
                                    if (i == 0 || i == 4 || i == 8 || i == 12 || i == 16 || i == 20)
                                    {
                                        printBuffer += "\r";
                                    }
                                    printBuffer += "ServiceIds[" + i + "]: " + asc.ServiceIds[i] + "        ";
                                }
                            }
                        }
                        printBuffer += "\r-----------------------------------------------------------------------------------------------------------------------------\r";

                        PrintEvents(printBuffer);


                        break;
                    }
                case CTIEventType.AgentLoginRequestFailed:
                    {
                        CTIAgentLoginRequestFailed alrf = ctiEvent as CTIAgentLoginRequestFailed;

                        
                           printBuffer= "Got AgentLoginRequestFailed Event:  FailureType: " + alrf.FailureType + "   FailureReason: " + alrf.FailureReason
                            + "\rRequestId: " + alrf.RequestId + "   SiteId: " + alrf.SiteId + "   SwitchId: " + alrf.SwitchId + "   TenantId: " + alrf.TenantId
                            + "\rAgentLoginName: " + alrf.AgentLoginName + "   AgentId: " + alrf.AgentId + "   ServiceId: " + alrf.ServiceId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r";

                           PrintEvents(printBuffer);


                        break;
                    }
                case CTIEventType.AgentRequestFailed:
                    {
                        CTIAgentRequestFailed arf = ctiEvent as CTIAgentRequestFailed;

                        printBuffer = "Got AgentRequestFailed Event:  FailureType: " + arf.FailureType + "   FailureReason: " + arf.FailureReason
                            + "\rRequestId: " + arf.RequestId + "   SiteId: " + arf.SiteId + "   SwitchId: " + arf.SwitchId + "   TenantId: " + arf.TenantId
                            + "\rAgentId: " + arf.AgentId + "   ServiceId: " + arf.ServiceId + "   CallId: " + arf.CallId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r";

                        PrintEvents(printBuffer);

                        break;
                    }
                case CTIEventType.AudioPathStateChange:
                    {
                        CTIAudioPathStateChange apsc = ctiEvent as CTIAudioPathStateChange;

                        printBuffer = "";

                        printBuffer += "Got AudioPathStateChange Event:  AgentId: " + apsc.AgentId + "   AudioPathState: " + apsc.AudioPathState
                            + "\rRequestId: " + apsc.RequestId + "   SiteId: " + apsc.SiteId + "   SwitchId: " + apsc.SwitchId + "   TenantId: " + apsc.TenantId
                            + "\rServiceId: " + apsc.ServiceId + "   SipIpAddress: " + apsc.SipIpAddress + "   SipPortNumber: " + apsc.SipPortNumber
                            + "\rRsrcGrpId: " + apsc.ResourceGroupId + "   CircuitId: " + apsc.CircuitId + "   ChannelId: " + apsc.ChannelId;

                        if (apsc.ServiceIds != null)
                        {
                            if (apsc.ServiceIds.Length > 0)
                            {
                                for (int i = 0; i < apsc.ServiceIds.Length; i++)
                                {
                                    if (i == 0 || i == 4 || i == 8 || i == 12 || i == 16 || i == 20)
                                    {
                                        printBuffer += "\r";
                                    }
                                    printBuffer += "ServiceIds[" + i + "]: " + apsc.ServiceIds[i] + "        ";
                                }
                            }
                        }
                        printBuffer += "\r-----------------------------------------------------------------------------------------------------------------------------\r";

                        PrintEvents(printBuffer);


                        break;
                    }
                case CTIEventType.EnterPasscode:
                    {
                        CTIEnterPasscode epc = ctiEvent as CTIEnterPasscode;

                        printBuffer = "Got EnterPasscode Event:  AgentId: " + epc.AgentId + "   PasscodeDigits: " + epc.PasscodeDigits + "   ReEnterFlag: " + epc.ReEnterFlag
                            + "\rRequestId: " + epc.RequestId + "   SiteId: " + epc.SiteId + "   SwitchId: " + epc.SwitchId + "   TenantId: " + epc.TenantId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r";

                        PrintEvents(printBuffer);

                        break;
                    }
                case CTIEventType.CallCleared:
                    {
                        CTICallCleared cc = ctiEvent as CTICallCleared;

                        printBuffer = "Got CallCleared Event:  CallId: " + cc.CallId + "   OrigSiteId: " + cc.CallOriginatingSiteId + "   CallSequenceId: " + cc.CallSequenceId
                            + "\rRequestId: " + cc.RequestId + "   SiteId: " + cc.SiteId + "   SwitchId: " + cc.SwitchId + "   TenantId: " + cc.TenantId
                            + "\rAgentId: " + cc.AgentId + "   ServiceId: " + cc.ServiceId + "   ConsultCallId: " + cc.ConsultationCallId
                            + "\rClearingPartyType: " + cc.ClearingPartyType
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r";

                        PrintEvents(printBuffer);
                        //_cadChannel.EndCall(cc.CallId);
                        break;
                    }

                case CTIEventType.CallCompleted:
                    {
                        CTICallCompleted ccp = ctiEvent as CTICallCompleted;

                        printBuffer = "Got CallCompleted Event:  CallId: " + ccp.CallId + "   OrigSiteId: " + ccp.CallOriginatingSiteId + "   CallSequenceId: " + ccp.CallSequenceId
                            + "\rRequestId: " + ccp.RequestId + "   SiteId: " + ccp.SiteId + "   SwitchId: " + ccp.SwitchId + "   TenantId: " + ccp.TenantId     
                            + "\rResourceGroupId: " + ccp.ResourceGroupId + "    CircuitId: " + ccp.CircuitId + "    ChannelId: " + ccp.ChannelId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r";

                        PrintEvents(printBuffer);
                        //_cadChannel.EndCall(ccp.CallId);
                        break;
                    }

                case CTIEventType.CallConferenced:
                    {
                        CTICallConferenced ccf = ctiEvent as CTICallConferenced;

                        PrintEvents(
                            "Got CallConferenced Event:  CallId: " + ccf.CallId + "   ConfCallId: " + ccf.ConferenceCallId + "   OrigSiteId: " + ccf.CallOriginatingSiteId + "   CallSequenceId: " + ccf.CallSequenceId
                            + "\rRequestId: " + ccf.RequestId + "   ServiceId: " + ccf.ServiceId + "   SiteId: " + ccf.SiteId + "   SwitchId: " + ccf.SwitchId + "   TenantId: " + ccf.TenantId
                            + "\rAgentId: " + ccf.AgentId + "   ConsultingCallId: " + ccf.ConsultingCallId + "   ConsultedCallId: " + ccf.ConsultedCallId + "   ConsultAgentId: " + ccf.ConsultationAgentId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");
                        
                        break;
                    }
                case CTIEventType.CallConnected:
                    {
                        CTICallConnected cce = ctiEvent as CTICallConnected;

                        PrintEvents(
                            "Got CallConnected Event:  CallId: " + cce.CallId + "   OrigSiteId: " + cce.CallOriginatingSiteId + "   CallSequenceId: " + cce.CallSequenceId
                            + "\rRequestId: " + cce.RequestId + "   ServiceId: " + cce.ServiceId + "   SiteId: " + cce.SiteId + "   SwitchId: " + cce.SwitchId + "   TenantId: " + cce.TenantId
                            + "\rAgentId: " + cce.AgentId + "   ResponseRequired: " + cce.ResponseRequired + "   RejectReasonRequired: " + cce.RejectReasonRequired + "   PlayAudioAlert: " + cce.PlayAudioAlert
                            + "\rOriginatingAgentId: " + cce.OriginatingAgentId + "   CallType: " + cce.CallType.ToString()
                            + "\rRsrcGrpId: " + cce.ResourceGroupId + "   CircuitId: " + cce.CircuitId + "   ChannelId: " + cce.ChannelId
                            + "\rDNIS: " + cce.CallInfo.DNIS + "   ANI: " + cce.CallInfo.ANI + "   Caller First Name: " + cce.CallInfo.FirstName + "   Caller Last Name: " + cce.CallInfo.LastName
                            + "\rCallData:     key1  = " + cce.CallInfo.CallData[0].key + "     value1  = " + cce.CallInfo.CallData[0].value
                            + "\rCallData:     key2  = " + cce.CallInfo.CallData[1].key + "     value2  = " + cce.CallInfo.CallData[1].value
                            + "\rCallData:     key3  = " + cce.CallInfo.CallData[2].key + "     value3  = " + cce.CallInfo.CallData[2].value
                            + "\rCallData:     key4  = " + cce.CallInfo.CallData[3].key + "     value4  = " + cce.CallInfo.CallData[3].value
                            + "\rCallData:     key5  = " + cce.CallInfo.CallData[4].key + "     value5  = " + cce.CallInfo.CallData[4].value
                            + "\rCallData:     key6  = " + cce.CallInfo.CallData[5].key + "     value6  = " + cce.CallInfo.CallData[5].value
                            + "\rCallData:     key7  = " + cce.CallInfo.CallData[6].key + "     value7  = " + cce.CallInfo.CallData[6].value
                            + "\rCallData:     key8  = " + cce.CallInfo.CallData[7].key + "     value8  = " + cce.CallInfo.CallData[7].value
                            + "\rCallData:     key9  = " + cce.CallInfo.CallData[8].key + "     value9  = " + cce.CallInfo.CallData[8].value
                            + "\rCallData:     key10 = " + cce.CallInfo.CallData[9].key + "     value10 = " + cce.CallInfo.CallData[9].value
                            + "\rCallData:     key11 = " + cce.CallInfo.CallData[10].key + "    value11 = " + cce.CallInfo.CallData[10].value
                            + "\rCallData:     key12 = " + cce.CallInfo.CallData[11].key + "    value12 = " + cce.CallInfo.CallData[11].value
                            + "\rCallData:     key13 = " + cce.CallInfo.CallData[12].key + "    value13 = " + cce.CallInfo.CallData[12].value
                            + "\rCallData:     key14 = " + cce.CallInfo.CallData[13].key + "    value14 = " + cce.CallInfo.CallData[13].value
                            + "\rCallData:     key15 = " + cce.CallInfo.CallData[14].key + "    value15 = " + cce.CallInfo.CallData[14].value
                            + "\rCallData:     key16 = " + cce.CallInfo.CallData[15].key + "    value16 = " + cce.CallInfo.CallData[15].value
                            + "\rCallData:     key17 = " + cce.CallInfo.CallData[16].key + "    value17 = " + cce.CallInfo.CallData[16].value
                            + "\rCallData:     key18 = " + cce.CallInfo.CallData[17].key + "    value18 = " + cce.CallInfo.CallData[17].value
                            + "\rCallData:     key19 = " + cce.CallInfo.CallData[18].key + "    value19 = " + cce.CallInfo.CallData[18].value
                            + "\rCallData:     key20 = " + cce.CallInfo.CallData[19].key + "    value20 = " + cce.CallInfo.CallData[19].value
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        var station = "";
                        if (_agents.ContainsKey(cce.AgentId))
                        {
                            station = _agents[cce.AgentId];
                        }

                        _cadChannel.NewInboundCall(cce.CallId, cce.CallInfo.ANI, station, cce.ResourceGroupId.ToString()); 

                        break;
                    }
                case CTIEventType.CallConsultation:
                    {
                        CTICallConsultation ctc = ctiEvent as CTICallConsultation;

                        PrintEvents(
                            "Got CallConsultation Event:  CallId: " + ctc.CallId + "   OrigSiteId: " + ctc.CallOriginatingSiteId + "   CallSequenceId: " + ctc.CallSequenceId
                            + "\rRequestId: " + ctc.RequestId + "   ServiceId: " + ctc.ServiceId + "   SiteId: " + ctc.SiteId + "   SwitchId: " + ctc.SwitchId + "   TenantId: " + ctc.TenantId
                            + "\rAgentId: " + ctc.AgentId + "   ConsultCallId: " + ctc.ConsultationCallId + "   ConsultAgentId: " + ctc.ConsultationAgentId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");
                        

                        break;
                    }
                case CTIEventType.CallDelivered:
                    {
                        CTICallDelivered cde = ctiEvent as CTICallDelivered;

                        PrintEvents(
                            "Got CallDelivered Event:  CallId: " + cde.CallId + "   OrigSiteId: " + cde.CallOriginatingSiteId + "   CallSequenceId: " + cde.CallSequenceId
                            + "\rRequestId: " + cde.RequestId + "   SiteId: " + cde.SiteId + "   SwitchId: " + cde.SwitchId + "   TenantId: " + cde.TenantId + "   ServiceId: " + cde.ServiceId
                            + "\rDNIS: " + cde.DNIS + "    ANI: " + cde.ANI + "   CallType: " + cde.CallType
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        
                        break;
                    }
                case CTIEventType.CallEnd:
                    {
                        CTICallEnd cee = ctiEvent as CTICallEnd;

                        PrintEvents(
                            "Got CallEnd Event:  CallId: " + cee.CallId + "   OrigSiteId: " + cee.CallOriginatingSiteId + "   CallSequenceId: " + cee.CallSequenceId
                            + "\rRequestId: " + cee.RequestId + "   SiteId: " + cee.SiteId + "   SwitchId: " + cee.SwitchId + "   TenantId: " + cee.TenantId
                            + "\rServiceId: " + cee.ServiceId + "   AgentId: " + cee.AgentId + "   AgentDisposition: " + cee.AgentDisposition
                            + "\rCallData:     key1  = " + cee.CallData[0].key + "     value1  = " + cee.CallData[0].value
                            + "\rCallData:     key2  = " + cee.CallData[1].key + "     value2  = " + cee.CallData[1].value
                            + "\rCallData:     key3  = " + cee.CallData[2].key + "     value3  = " + cee.CallData[2].value
                            + "\rCallData:     key4  = " + cee.CallData[3].key + "     value4  = " + cee.CallData[3].value
                            + "\rCallData:     key5  = " + cee.CallData[4].key + "     value5  = " + cee.CallData[4].value
                            + "\rCallData:     key6  = " + cee.CallData[5].key + "     value6  = " + cee.CallData[5].value
                            + "\rCallData:     key7  = " + cee.CallData[6].key + "     value7  = " + cee.CallData[6].value
                            + "\rCallData:     key8  = " + cee.CallData[7].key + "     value8  = " + cee.CallData[7].value
                            + "\rCallData:     key9  = " + cee.CallData[8].key + "     value9  = " + cee.CallData[8].value
                            + "\rCallData:     key10 = " + cee.CallData[9].key + "     value10 = " + cee.CallData[9].value
                            + "\rCallData:     key11 = " + cee.CallData[10].key + "    value11 = " + cee.CallData[10].value
                            + "\rCallData:     key12 = " + cee.CallData[11].key + "    value12 = " + cee.CallData[11].value
                            + "\rCallData:     key13 = " + cee.CallData[12].key + "    value13 = " + cee.CallData[12].value
                            + "\rCallData:     key14 = " + cee.CallData[13].key + "    value14 = " + cee.CallData[13].value
                            + "\rCallData:     key15 = " + cee.CallData[14].key + "    value15 = " + cee.CallData[14].value
                            + "\rCallData:     key16 = " + cee.CallData[15].key + "    value16 = " + cee.CallData[15].value
                            + "\rCallData:     key17 = " + cee.CallData[16].key + "    value17 = " + cee.CallData[16].value
                            + "\rCallData:     key18 = " + cee.CallData[17].key + "    value18 = " + cee.CallData[17].value
                            + "\rCallData:     key19 = " + cee.CallData[18].key + "    value19 = " + cee.CallData[18].value
                            + "\rCallData:     key20 = " + cee.CallData[19].key + "    value20 = " + cee.CallData[19].value
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        _cadChannel.EndCall(cee.CallId);
                        break;
                    }
                case CTIEventType.CallEstablished:
                    {
                        CTICallEstablished cee = ctiEvent as CTICallEstablished;

                        PrintEvents(
                            "Got CallEstablished Event:  CallId: " + cee.CallId + "   OrigSiteId: " + cee.CallOriginatingSiteId + "   CallSequenceId: " + cee.CallSequenceId
                            + "\rRequestId: " + cee.RequestId + "   SiteId: " + cee.SiteId + "   SwitchId: " + cee.SwitchId + "   TenantId: " + cee.TenantId
                            + "\rAgentId: " + cee.AgentId + "   ServiceId: " + cee.ServiceId + "   SipIpAddress: " + cee.SipIpAddress + "   SipPortNumber: " + cee.SipPortNumber
                            + "\rRsrcGrpId: " + cee.ResourceGroupId + "   CircuitId: " + cee.CircuitId + "   ChannelId: " + cee.ChannelId + "   MediaType: " + cee.MediaType
                           + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        /// find the agents' station
                        /// 

                        if (_agents.ContainsKey( cee.AgentId ))
                        {
                            var station = _agents[ cee.AgentId ];
                            Logger.Write(String.Format("Channel {0} Call established id={1} on {2} for agent {3}",_channelName, cee.CallId, station, cee.AgentId), TraceEventType.Information, "CTIPSChannel");
                            _cadChannel.Connected(cee.CallId, station);
                        }
                        else
                        {
                            Logger.Write(String.Format("Channel {0} Call established id={1} for agent {2} but no record of which station they are using", _channelName, cee.CallId, cee.AgentId), TraceEventType.Information, "CTIPSChannel");
                        }
                        break;
                    }
                case CTIEventType.CallDataUpdate:
                    {
                        CTICallDataUpdate cdu = ctiEvent as CTICallDataUpdate;

                        PrintEvents(
                            "Got CallDataUpdate Event:  CallId: " + cdu.CallId + "   OrigSiteId: " + cdu.CallOriginatingSiteId + "   CallSequenceId: " + cdu.CallSequenceId
                            + "\rRequestId: " + cdu.RequestId + "   SiteId: " + cdu.SiteId + "   SwitchId: " + cdu.SwitchId + "   TenantId: " + cdu.TenantId
                            + "\rAgentId: " + cdu.AgentId + "   ServiceId: " + cdu.ServiceId + "   UpdateType: " + cdu.UpdateType
                            + "\rCallData:     key1  = " + cdu.CallData[0].key + "     value1  = " + cdu.CallData[0].value
                            + "\rCallData:     key2  = " + cdu.CallData[1].key + "     value2  = " + cdu.CallData[1].value
                            + "\rCallData:     key3  = " + cdu.CallData[2].key + "     value3  = " + cdu.CallData[2].value
                            + "\rCallData:     key4  = " + cdu.CallData[3].key + "     value4  = " + cdu.CallData[3].value
                            + "\rCallData:     key5  = " + cdu.CallData[4].key + "     value5  = " + cdu.CallData[4].value
                            + "\rCallData:     key6  = " + cdu.CallData[5].key + "     value6  = " + cdu.CallData[5].value
                            + "\rCallData:     key7  = " + cdu.CallData[6].key + "     value7  = " + cdu.CallData[6].value
                            + "\rCallData:     key8  = " + cdu.CallData[7].key + "     value8  = " + cdu.CallData[7].value
                            + "\rCallData:     key9  = " + cdu.CallData[8].key + "     value9  = " + cdu.CallData[8].value
                            + "\rCallData:     key10 = " + cdu.CallData[9].key + "     value10 = " + cdu.CallData[9].value
                            + "\rCallData:     key11 = " + cdu.CallData[10].key + "    value11 = " + cdu.CallData[10].value
                            + "\rCallData:     key12 = " + cdu.CallData[11].key + "    value12 = " + cdu.CallData[11].value
                            + "\rCallData:     key13 = " + cdu.CallData[12].key + "    value13 = " + cdu.CallData[12].value
                            + "\rCallData:     key14 = " + cdu.CallData[13].key + "    value14 = " + cdu.CallData[13].value
                            + "\rCallData:     key15 = " + cdu.CallData[14].key + "    value15 = " + cdu.CallData[14].value
                            + "\rCallData:     key16 = " + cdu.CallData[15].key + "    value16 = " + cdu.CallData[15].value
                            + "\rCallData:     key17 = " + cdu.CallData[16].key + "    value17 = " + cdu.CallData[16].value
                            + "\rCallData:     key18 = " + cdu.CallData[17].key + "    value18 = " + cdu.CallData[17].value
                            + "\rCallData:     key19 = " + cdu.CallData[18].key + "    value19 = " + cdu.CallData[18].value
                            + "\rCallData:     key20 = " + cdu.CallData[19].key + "    value20 = " + cdu.CallData[19].value
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.CallHeld:
                    {
                        CTICallHeld che = ctiEvent as CTICallHeld;

                        PrintEvents(
                            "Got CallHeld Event:  CallId: " + che.CallId + "   OrigSiteId: " + che.CallOriginatingSiteId + "   CallSequenceId: " + che.CallSequenceId
                            + "\rRequestId: " + che.RequestId + "   SiteId: " + che.SiteId + "   SwitchId: " + che.SwitchId + "   TenantId: " + che.TenantId
                            + "\rAgentId: " + che.AgentId + "   ServiceId: " + che.ServiceId
                           + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.CallOffered:
                    {
                        CTICallOffered coe = ctiEvent as CTICallOffered;

                        PrintEvents(
                            "Got CallOffered Event:  CallId: " + coe.CallId + "   OrigSiteId: " + coe.CallOriginatingSiteId + "   CallSequenceId: " + coe.CallSequenceId
                            + "\rRequestId: " + coe.RequestId + "   SiteId: " + coe.SiteId + "   SwitchId: " + coe.SwitchId + "   TenantId: " + coe.TenantId
                            + "\rAgentId: " + coe.AgentId + "   ServiceId: " + coe.ServiceId
                            + "\rRsrcGrpId: " + coe.ResourceGroupId + "   CircuitId: " + coe.CircuitId + "   ChannelId: " + coe.ChannelId
                           + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.CallOriginated:
                    {
                        CTICallOriginated co = ctiEvent as CTICallOriginated;

                        PrintEvents(
                            "Got CallOriginated Event:  CallId: " + co.CallId + "   OrigSiteId: " + co.CallOriginatingSiteId + "   CallSequenceId: " + co.CallSequenceId
                            + "\rRequestId: " + co.RequestId + "   SiteId: " + co.SiteId + "   SwitchId: " + co.SwitchId + "   TenantId: " + co.TenantId
                            + "\rAgentId: " + co.AgentId + "   ServiceId: " + co.ServiceId + "   DestType: " + co.DestType + "   Dest: " + co.Destination
                            + "\rRsrcGrpId: " + co.ResourceGroupId + "   CircuitId: " + co.CircuitId + "   ChannelId: " + co.ChannelId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        _cadChannel.NewOutboundCall(co.CallId, co.Destination, co.ResourceGroupId.ToString()); 

                        break;
                    }
                case CTIEventType.DigitsDialed:
                    {
                        CTIDigitsDialed dde = ctiEvent as CTIDigitsDialed;

                        PrintEvents(
                            "Got DigitsDialed Event:  CallId: " + dde.CallId + "   OrigSiteId: " + dde.CallOriginatingSiteId + "   CallSequenceId: " + dde.CallSequenceId
                            + "\rRequestId: " + dde.RequestId + "   SiteId: " + dde.SiteId + "   SwitchId: " + dde.SwitchId + "   TenantId: " + dde.TenantId
                            + "\rAgentId: " + dde.AgentId + "   ServiceId: " + dde.ServiceId + "   DigitString: " + dde.DigitString
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.CallRequestFailed:
                    {
                        CTICallRequestFailed crf = ctiEvent as CTICallRequestFailed;

                        PrintEvents(
                            "Got CallRequestFailed Event:  FailureType: " + crf.FailureType + "   FailureReason: " + crf.FailureReason
                            + "\rCallId: " + crf.CallId + "   OrigSiteId: " + crf.CallOriginatingSiteId + "   CallSequenceId: " + crf.CallSequenceId
                            + "\rRequestId: " + crf.RequestId + "   SiteId: " + crf.SiteId + "   SwitchId: " + crf.SwitchId + "   TenantId: " + crf.TenantId
                            + "\rAgentId: " + crf.AgentId + "   ServiceId: " + crf.ServiceId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.CallRetrieved:
                    {
                        CTICallRetrieved cre = ctiEvent as CTICallRetrieved;

                        PrintEvents(
                            "Got CallRetrieved Event:  CallId: " + cre.CallId + "   OrigSiteId: " + cre.CallOriginatingSiteId + "   CallSequenceId: " + cre.CallSequenceId
                            + "\rRequestId: " + cre.RequestId + "   SiteId: " + cre.SiteId + "   SwitchId: " + cre.SwitchId + "   TenantId: " + cre.TenantId
                            + "\rAgentId: " + cre.AgentId + "   ServiceId: " + cre.ServiceId
                           + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }

                case CTIEventType.CallStarted:
                    {
                        CTICallStarted cs = ctiEvent as CTICallStarted;

                        PrintEvents(
                            "Got CallStarted Event:  CallId: " + cs.CallId + "   OrigSiteId: " + cs.CallOriginatingSiteId + "   CallSequenceId: " + cs.CallSequenceId
                            + "\rRequestId: " + cs.RequestId + "   SiteId: " + cs.SiteId + "   SwitchId: " + cs.SwitchId + "   TenantId: " + cs.TenantId
                            + "\rCircuitId: " + cs.CircuitId + "    ChannelId: " + cs.ChannelId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }

                case CTIEventType.CallTransferred:
                    {
                        CTICallTransferred cte = ctiEvent as CTICallTransferred;

                        PrintEvents(
                            "Got CallTransferred Event:  CallId: " + cte.CallId + "   OrigSiteId: " + cte.CallOriginatingSiteId + "   CallSequenceId: " + cte.CallSequenceId
                            + "\rRequestId: " + cte.RequestId + "   SiteId: " + cte.SiteId + "   SwitchId: " + cte.SwitchId + "   TenantId: " + cte.TenantId
                            + "\rAgentId: " + cte.AgentId + "   ServiceId: " + cte.ServiceId + "   ConsultingCallId: " + cte.ConsultingCallId + "   ConsultedCallId: " + cte.ConsultedCallId
                            + "\rConsultAgentId: " + cte.ConsultationAgentId + "   TransfCallId: " + cte.TransferredCallId + "   TransfCallSwitchId: " + cte.TransferredCallSwitchId
                           + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.RouteReject:
                    {
                        CTIRouteRejected rrje = ctiEvent as CTIRouteRejected;

                        PrintEvents(
                            "Got RouteReject:  CallId: " + rrje.CallId + "   RouteRejectReason: " + rrje.RouteRejectReason + "   ServiceId: " + rrje.ServiceId + "   AgentId: " + rrje.AgentId
                            + "\rRequestId: " + rrje.RequestId + "   SiteId: " + rrje.SiteId + "   SwitchId: " + rrje.SwitchId + "   TenantId: " + rrje.TenantId
                            + "\rOrigSiteId: " + rrje.CallOriginatingSiteId + "   CallSequenceId: " + rrje.CallSequenceId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.RouteRequest:
                    {
                        CTIRouteRequest rre = ctiEvent as CTIRouteRequest;

                        PrintEvents(
                            "Got RouteRequest Event:  CallId: " + rre.CallId + "   RouteTimeout: " + rre.RouteTimeout + "   ServiceId: " + rre.ServiceId + "   AgentId: " + rre.AgentId
                            + "\rRequestId: " + rre.RequestId + "   SiteId: " + rre.SiteId + "   SwitchId: " + rre.SwitchId + "   TenantId: " + rre.TenantId + "   MediaType: " + rre.MediaType
                            + "\rRsrcGrpId: " + rre.ResourceGroupId + "   CircuitId: " + rre.CircuitId + "   ChannelId: " + rre.ChannelId + "   SipIpAddress: " + rre.SipIpAddress + "   SipPortNumber: " + rre.SipPortNumber
                            + "\rDNIS: " + rre.CallInfo.DNIS + "   ANI: " + rre.CallInfo.ANI + "   OrigSiteId: " + rre.CallOriginatingSiteId + "   CallSequenceId: " + rre.CallSequenceId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.RouteRequestFailed:
                    {
                        CTIRouteRequestFailed rrf = ctiEvent as CTIRouteRequestFailed;

                        PrintEvents(
                            "Got RouteRequestFailed Event: FailureType: " + rrf.FailureType + "   FailureReason: " + rrf.FailureReason
                            + "\rCallId: " + rrf.CallId + "   AgentId: " + rrf.AgentId + "   ServiceId: " + rrf.ServiceId
                            + "\rRequestId: " + rrf.RequestId + "   SiteId: " + rrf.SiteId + "   SwitchId: " + rrf.SwitchId + "   TenantId: " + rrf.TenantId
                            + "\rOrigSiteId: " + rrf.CallOriginatingSiteId + "   CallSequenceId: " + rrf.CallSequenceId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.RouteUsed:
                    {
                        CTIRouteUsed rue = ctiEvent as CTIRouteUsed;

                        PrintEvents(
                            "Got RouteUsed Event:  CallId: " + rue.CallId + "   RouteType: " + rue.RouteType + "   RouteDest: " + rue.RouteDestination
                            + "\rRouteStatus: " + rue.RouteStatus + "   AgentId: " + rue.AgentId + "   ServiceId: " + rue.ServiceId
                            + "\rRequestId: " + rue.RequestId + "   SiteId: " + rue.SiteId + "   SwitchId: " + rue.SwitchId + "   TenantId: " + rue.TenantId
                            + "\rOrigSiteId: " + rue.CallOriginatingSiteId + "   CallSequenceId: " + rue.CallSequenceId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.LinkStatus:
                    {
                        CTILinkStatus lse = ctiEvent as CTILinkStatus;

                        PrintEvents(
                            "Got LinkStatus Event:  ComponentType: " + lse.ComponentType + "ComponentState: " + lse.ComponentState
                            + "\rSiteId: " + lse.SiteId + "   SwitchId: " + lse.SwitchId + "   TenantId: " + lse.TenantId
                            + "\rRequestId: " + lse.RequestId + "   ServiceId: " + lse.ServiceId + "   AgentId: " + lse.AgentId
                            + "\r-----------------------------------------------------------------------------------------------------------------------------\r");

                        break;
                    }
                case CTIEventType.ServiceStateChange:
                    {
                        CTIServiceStateChange ssce = ctiEvent as CTIServiceStateChange;
                        printBuffer = "";

                        printBuffer += "Got ServiceStateChange Event:  ServiceId: " + ssce.ServiceId + "   ServicetState: " + ssce.ServiceState
                            + "\rRequestId: " + ssce.RequestId + "   AgentId: " + ssce.AgentId + "   SiteId: " + ssce.SiteId + "   SwitchId: " + ssce.SwitchId + "   TenantId: " + ssce.TenantId;

                        if (ssce.AgentIdList != null)
                        {
                            if (ssce.AgentIdList.Length > 0)
                            {
                                for (int i = 0; i < ssce.AgentIdList.Length; i++)
                                {
                                    if (i == 0 || i == 4 || i == 8 || i == 12 || i == 16 || i == 20 || i == 24 || i == 28 || i == 32)
                                    {
                                        printBuffer += "\r";
                                    }
                                    printBuffer += "AgentIdList[" + i + "]: " + ssce.AgentIdList[i] + "   ";
                                }
                            }
                        }
                        printBuffer += "\r-----------------------------------------------------------------------------------------------------------------------------\r";
                        PrintEvents(printBuffer);

                        break;
                    }
                case CTIEventType.SnapShotUpdate:
                    {
                        CTISnapShotUpdate ssu = ctiEvent as CTISnapShotUpdate;
                        if (ssu.SnapShotType == CTIEntityType.AgentInfo)
                        {
                            AddAgent(ssu.AgentData.AgentStationAddress, ssu.AgentData.AgentId);

                            printBuffer = "";
                            printBuffer += "Got SnapShotUpdate Agent Event:  AgentId: " + ssu.AgentData.AgentId + "   AgentLoginName: " + ssu.AgentData.AgentLoginName + "   StationId: " + ssu.AgentData.AgentStationAddress + "   AgentState: " + ssu.AgentData.AgentState.ToString()                            
                                + "\rRequestId: " + ssu.RequestId + "   SiteId: " + ssu.SiteId + "   TenantId: " + ssu.TenantId + "   RsrcGrpId: " + ssu.AgentData.ResourceGroupId + "   CircuitId: " + ssu.AgentData.CircuitId + "   ChannelId: " + ssu.AgentData.ChannelId
                                + "\rAgentWorkGrpId: " + ssu.AgentData.AgentWorkGroupId + "   Agent MediaType: " + ssu.AgentData.MediaType + "   CallId: " + ssu.AgentData.CallId + "   SnapShotTypeComplete: " + ssu.SnapShotTypeComplete;
                            if (ssu.AgentData.Services != null)
                            {
                                if (ssu.AgentData.Services.Length > 0)
                                {
                                    for (int i = 0; i < ssu.AgentData.Services.Length; i++)
                                    {
                                        if (i == 0 || i == 2 || i == 4 || i == 6 || i == 8 || i == 10 || i == 12)
                                        {
                                            printBuffer += "\r";
                                        }
                                        printBuffer += "ServiceID[" + i + "]: " + ssu.AgentData.Services[i].ServiceId + "   ServiceState[" + i + "]: " + ssu.AgentData.Services[i].ServiceState + "       ";
                                    }
                                }
                            }
                            if (ssu.AgentData.AgentCapabilities != null)
                            {
                                for (int i = 0; i < ssu.AgentData.AgentCapabilities.Length; i++)
                                {
                                    if (ssu.AgentData.AgentCapabilities[i] != CTIAgentCapabilities.INVALID)
                                    {
                                        if (i == 0 || i == 2 || i == 4)
                                        {
                                            printBuffer += "\r";
                                        }
                                        printBuffer += "AgentCapabilities[" + i + "]: " + ssu.AgentData.AgentCapabilities[i] + "            ";
                                    }
                                }
                            }
                            printBuffer += "\r-----------------------------------------------------------------------------------------------------------------------------\r";

                            PrintEvents(printBuffer);

                        }
                        else if (ssu.SnapShotType == CTIEntityType.CallInfo)
                        {
                            PrintEvents(
                                "Got SnapShotUpdate Call Event:  CallId: " + ssu.CallData.CallId + "   CallType: " + ssu.CallData.CallType + "   CallState: " + ssu.CallData.CallState
                                + "\rRequestId: " + ssu.RequestId + "   SiteId: " + ssu.SiteId + "   TenantId: " + ssu.TenantId + "   RsrcGrpId: " + ssu.CallData.ResourceGroupId + "   CircuitId: " + ssu.CallData.CircuitId + "   ChannelId: " + ssu.CallData.ChannelId
                                + "\rOrigSiteId: " + ssu.CallData.CallOriginatingSiteId + "   CallSeqId: " + ssu.CallData.CallSequenceId + "   ANI: " + ssu.CallData.ANI + "   DNIS: " + ssu.CallData.DNIS
                                + "\rSwitchId: " + ssu.CallData.SwitchId + "   AgentId: " + ssu.CallData.AgentId + "   ServiceId: " + ssu.CallData.ServiceId + "   SipIpAddress: " + ssu.CallData.SipIpAddress + "   SipPortNumber: " + ssu.CallData.SipPortNumber
                                + "\rMediaType: " + ssu.CallData.MediaType + "   DestType: " + ssu.CallData.DestType + "   DestAddress: " + ssu.CallData.Destination + "   SnapShotTypeComplete: " + ssu.SnapShotTypeComplete
                                + "\r-----------------------------------------------------------------------------------------------------------------------------\r");
                        }
                        else if (ssu.SnapShotType == CTIEntityType.ServiceInfo)
                        {
                            PrintEvents(
                                "Got SnapShotUpdate Service Event:  ServiceId: " + ssu.ServiceData.ServiceId + "   ServiceState: " + ssu.ServiceData.ServiceState.ToString()
                                + "\rRequestId: " + ssu.RequestId + "   SiteId: " + ssu.SiteId + "   TenantId: " + ssu.TenantId + "   SnapShotComplete: " + ssu.SnapShotTypeComplete
                                + "\r-----------------------------------------------------------------------------------------------------------------------------\r");
                        }
                        else
                        {
                        }
                        break;
                    }
                default:
                    {
                        PrintEvents("Got Unknown Event Type \r");
                    }
                    break;
            }
            

            return res;
        }

        /// <summary>
        /// process a heartbeat
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public int Heartbeat(string sessionId)
        {
            int res = 0;
            
            PrintEvents("------ Got Heartbeat from CTIPS -------\r");
            
            if (HeartbeatEvent!=null)
                HeartbeatEvent(this, null);

            return res;
        }

        /// <summary>
        /// get the agent Id based on which station they are logged in on
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        public int GetAgentID(String stationId)
        {
            stationId = NumericOnly(stationId);
            Logger.Write(string.Format("Lookup agent on station {0}", stationId), TraceEventType.Information, "CTIPSChannel");
            foreach (var a in _agents)
            {
                Logger.Write(string.Format("    checking key={0} value={1}", a.Key, a.Value), TraceEventType.Information, "CTIPSChannel");
                if (a.Value.ToLower() == stationId.ToLower())
                {
                    Logger.Write(string.Format("    found key={0} value={1}={2}", a.Key, a.Value, stationId), TraceEventType.Information, "CTIPSChannel");
                    return a.Key;
                }
            }
            Logger.Write(string.Format("Lookup agent.. no agent on station {0}", stationId), TraceEventType.Information, "CTIPSChannel");
            return -1;
        }

        string NumericOnly(String txt)
        {
            string result = "";
            foreach( var c in txt.ToCharArray())
            {
                if ("0987654321".Contains(c))
                    result += c;
            }
            return result;
        }


        /// <summary>
        /// flush all agents out of the cache
        /// </summary>
        public void FlushAgents()
        {
            _agents.Clear();
        }

        /// <summary>
        /// remove an agent from the cache
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="agentId"></param>
        private void RemoveAgent(String stationAddress, int agentId)
        {
            // remove any entries that have this agenid or station address
            _agents
                .Where(x => x.Value == stationAddress || x.Key == agentId)
                .Select(x => x.Key)
                .ToList()
                .ForEach(x => _agents.Remove(x));
        }

        /// <summary>
        /// add an agent to the cache
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="agentId"></param>
        private void AddAgent(String stationAddress, int agentId)
        {
            stationAddress = NumericOnly(stationAddress);

            RemoveAgent(stationAddress, agentId);

            if (agentId == 0)
                return;

            _agents.Add(agentId, stationAddress);

            Logger.Write(string.Format("Channel {0} Added agent {1}<-->{2} total associations={3} ", this.ToString(), agentId, stationAddress, _agents.Count), TraceEventType.Information, "CTIPSChannel");
            Logger.Write(string.Format("--- Agent List --- "), TraceEventType.Information, "CTIPSChannel");
            foreach (var a in _agents)
            {
                Logger.Write(string.Format("Channel {0}       agent {1}<-->{2}", this.ToString(), a.Key, a.Value), TraceEventType.Information, "CTIPSChannel");
            }
        }
        
        public int StartSession(UserCredentials userinfo, string sessionId)
        {
            PrintEvents("------ StartSession called. Username: " + userinfo.UserName + " password: " + userinfo.Password + " -------\r");
            return 0;
        }

        #endregion
    }
}
