using Quest.Mobile.Code;
using Quest.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http;
using Quest.Common.Messages;
#if OAUTH
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
#endif

namespace Quest.Mobile.Controllers
{
    /// <summary>
    /// This class acts as an interface to mobile devices.
    /// </summary>
    public class DeviceController : ApiController
    {
        [ActionName("Submit")]
        public Response Submit(Request request)
        {
            Response result = null;
            try
            {
                result = MvcApplication.MsgClientCache.SendAndWait<Response>(request, new TimeSpan(0, 0, 3));
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Application", "Quest Mobile: " + ex);
            }
            return result;
        }

        [ActionName("Login")]        
        public LoginResponse Login(LoginRequest request)
        {
            LoginResponse result = null;
            try
            {
#if OAUTH
                var context = new ApplicationDbContext();
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var user = userManager.FindByName(request.Username);
                var isValidUser = (user != null);
                var isValidPassword = false;

                if (isValidUser)
                    isValidPassword = userManager.CheckPassword(user, request.Password);
#else
                var isValidPassword = true;
                var user = request.Username;
#endif

                // we can check the login request user credentials here
                if (isValidPassword==false)
                {
                    return new LoginResponse() { 
                        AuthToken = "",
                        QuestApi = "",
                        RequestId = request.RequestId,
                        RequiresCallsign = false,
                        Callsign = "",
                        Status = null,
                        Success = false, 
                        Message = "incorrect username or password" 
                    };
                }
                else
                {
                    result = request.Submit<LoginResponse>();
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse() { Success = false, Message = "failed to log on: "+ex.Message  };
            }
            finally
            {
            }
            return result;
        }

        [ActionName("Logout")]
        public LogoutResponse Logout(LogoutRequest request)
        {
            LogoutResponse result;
            try
            {
                result = request.Submit<LogoutResponse>();
            }
            catch (Exception ex)
            {
                return new LogoutResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }

        [ActionName("CallsignChange")]
        public CallsignChangeResponse CallsignChange(CallsignChangeRequest request)
        {
            CallsignChangeResponse result;
            try
            {
                result = request.Submit<CallsignChangeResponse>();
            }
            catch (Exception ex)
            {
                return new CallsignChangeResponse() { Success = false, Message = ex.Message };

            }
            return result;
        }

        [ActionName("RefreshState")]
        public RefreshStateResponse RefreshState(RefreshStateRequest request)
        {
            RefreshStateResponse result;
            try
            {
                result = request.Submit<RefreshStateResponse>();
            }
            catch (Exception ex)
            {
                return new RefreshStateResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }

        [ActionName("AckAssignedEvent")]
        public AckAssignedEventResponse AckAssignedEvent(AckAssignedEventRequest request)
        {
            AckAssignedEventResponse result;
            try
            {
                result = request.Submit<AckAssignedEventResponse>();
            }
            catch (Exception ex)
            {
                return new AckAssignedEventResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }

        [ActionName("PositionUpdate")]        
        public PositionUpdateResponse PositionUpdate(PositionUpdateRequest request)
        {
            PositionUpdateResponse result;
            try
            {
                result = request.Submit<PositionUpdateResponse>();
            }
            catch (Exception ex)
            {
                return new PositionUpdateResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }

        [ActionName("MakePatientObservation")]
        public MakePatientObservationResponse MakePatientObservation(MakePatientObservationRequest request)
        {
            MakePatientObservationResponse result;
            try
            {
                result = request.Submit<MakePatientObservationResponse>();
            }
            catch (Exception ex)
            {
                return new MakePatientObservationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
            return result;
        }

        [ActionName("PatientDetails")]
        public PatientDetailsResponse PatientDetails(PatientDetailsRequest request)
        {
            PatientDetailsResponse result;
            try
            {
                result = request.Submit<PatientDetailsResponse>();
            }
            catch (Exception ex)
            {
                return new PatientDetailsResponse() { Success = false, Message = ex.Message };

            }
            return result;
        }

        [ActionName("SetStatus")]
        public SetStatusResponse SetStatus(SetStatusRequest request)
        {
            SetStatusResponse result;
            try
            {
                result = request.Submit<SetStatusResponse>();
            }
            catch (Exception ex)
            {
                return new SetStatusResponse() { Success = false, Message = ex.Message };
            }
            return result;
        }

        [ActionName("GetStatusCodes")]
        public GetStatusCodesResponse GetStatusCodes(GetStatusCodesRequest request)
        {
            GetStatusCodesResponse result;
            try
            {
                result = request.Submit<GetStatusCodesResponse>();
            }
            catch (Exception ex)
            {
                return new GetStatusCodesResponse() { Success = false, Message = ex.Message };
            }
            return result;
        }

        [ActionName("GetEntityTypes")]
        public GetEntityTypesResponse GetEntityTypes(GetEntityTypesRequest request)
        {
            GetEntityTypesResponse result;
            try
            {
                result = request.Submit<GetEntityTypesResponse>();
            }
            catch (Exception ex)
            {
                return new GetEntityTypesResponse() { Success = false, Message = ex.Message };

            }
            return result;
        }

        [ActionName("GetMapItems")]
        [HttpPost]
        public MapItemsResponse GetMapItems(MapItemsRequest request)
        {
            MapItemsResponse result;
            try
            {
                result = request.Submit<MapItemsResponse>();
            }
            catch (Exception ex)
            {
                return new MapItemsResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
            return result;
        }

        [ActionName("GetHistory")]
        public GetHistoryResponse GetHistory(GetHistoryRequest request)
        {
            GetHistoryResponse result;
            try
            {
                result = request.Submit<GetHistoryResponse>();
            }
            catch (Exception ex)
            {
                return new GetHistoryResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }


        [ActionName("Help")]
        [HttpGet]
        public object Help()
        {
            var o = new
            {
                LoginRequest = new LoginRequest()
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    NotificationTypeId = 2,
                    Locale = "en-GB",
                    Password="mysecret",
                    Username="bob",
                },
                LoginResponse = new LoginResponse()
                {
                    AuthToken = Guid.NewGuid().ToString()
                },
                LogoutRequest = new LogoutRequest(),
                LogoutResponse = new LogoutResponse(),
                CallsignChangeRequest = new CallsignChangeRequest()
                {
                    Callsign = "G460",
                },
                CallsignChangeResponse = new CallsignChangeResponse(),
                GetAssignedEventRequest = new RefreshStateRequest(),
                GetStateResponse = new RefreshStateResponse(),
                PositionUpdateRequest = new PositionUpdateRequest()
                { 
                    Vector = new LocationVector()
                    {
                         AltitudeM=200,
                         BearingDeg=0,
                         CaptureMethod="Wireless",
                         HDoP=10,
                         VDoP=30,
                         Latitude = 52.1,
                         Longitude = -0.01,
                         SpeedMS = 4.2                          
                    }                     
                },
                PositionUpdateResponse = new PositionUpdateResponse(),
                MakePatientObservationRequest = new MakePatientObservationRequest()
                {
                    Observation = new PatientObservation()
                    {
                         Diastolic = 90,
                         Systolic = 140,
                         GCS = 15,
                         SATS = 94
                    }
                },
                MakePatientObservationResponse = new MakePatientObservationResponse(),
                PatientDetailsRequest = new PatientDetailsRequest()
                {
                    DoB ="03/Mar/1989"    ,
                    FirstName = "Siobhan",
                    LastName="?",
                    NHSNumber="?"                     
                },
                PatientDetailsResponse = new PatientDetailsResponse()
                {
                    DoB = "03/Mar/1989",
                    FirstName = "Siobhan",
                    LastName = "Metcalfe-Poulton",
                    NHSNumber = "NI987654321",
                    Notes = new List<string>() { "line 1", "line 2", "line 3 etc" },
                },
                SetStatusRequest = new SetStatusRequest()
                {
                    CorrespondingEventId = "L26061400001",
                    StatusCode = "ENR"
                     
                },
                SetStatusResponse = new SetStatusResponse()
                {
                    OldStatusCode = new StatusCode() { Code = "AOR", Description = "Available" },
                    NewStatusCode = new StatusCode() { Code = "ENR", Description = "Enroute" },
                },
                GetStatusCodesRequest = new GetStatusCodesRequest()
                {
                     SearchMode = GetStatusCodesRequest.Mode.AllCodes,
                     FromStatus =""
                },                
                GetEntityTypesRequest = new GetEntityTypesRequest()
                {
                },
                GetEntityTypesResponse = new GetEntityTypesResponse()
                {
                     Items = new List<string>() { "Stations", "Hospitals (non-A&E)", "Hospital (A&E)", "Hospitals (Maternity)", "Fuel", "A-Z Grid", "CCG", "Atoms"}
                },
                GetHistoryRequest = new GetHistoryRequest()
                {

                },
                GetHistoryResponse = new GetHistoryResponse()
                {
                    Items = new List<DeviceHistoryItem>()
                     {
                         new  DeviceHistoryItem(){ Callsign = "G460", EventId ="L0987654321", Notes="Dispatched", Status = "ENR", TimeStamp = DateTime.Now, Vector = null},
                         new  DeviceHistoryItem(){ Callsign = "G460", EventId ="L0987654321", Notes="On scene", Status = "ONS", TimeStamp = DateTime.Now, Vector = null},
                         new  DeviceHistoryItem(){ Callsign = "G460", EventId ="L0987654321", Notes="At Hospital", Status = "TAR", TimeStamp = DateTime.Now, Vector = null},
                         new  DeviceHistoryItem(){ Callsign = "G460", EventId ="", Notes="Available for work", Status = "AIQ", TimeStamp = DateTime.Now, Vector = null},
                         new  DeviceHistoryItem(){ Callsign = "G460", EventId ="L0987651231", Notes="Dispatched", Status = "ENR", TimeStamp = DateTime.Now, Vector = null},
                         new  DeviceHistoryItem(){ Callsign = "G460", EventId ="L0987651231", Notes="On scene", Status = "ONS", TimeStamp = DateTime.Now, Vector = null},
                         new  DeviceHistoryItem(){ Callsign = "G460", EventId ="L0987651231", Notes="At Hospital", Status = "TAR", TimeStamp = DateTime.Now, Vector = null},
                         new  DeviceHistoryItem(){ Callsign = "G460", EventId ="", Notes="Available for work", Status = "AIQ", TimeStamp = DateTime.Now, Vector = null}                         
                     }
                },
            };

            return o;
        }



    }

}