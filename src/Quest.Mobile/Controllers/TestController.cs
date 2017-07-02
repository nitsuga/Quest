using Quest.Mobile.Code;
using Quest.Mobile.Models;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Quest.Mobile.Controllers
{
    public class TestController : Controller
    {
        public TestController()
        {
        }

        //
        // GET: /Test/
        public ActionResult Index()
        {
            var message = Session["TestMessage"] as TestMessage;
            if (message==null)
            {
                message = new TestMessage();
                Session["TestMessage"] = message;
                
                var loc = GeoLocationService.GetLocationInfo();
                
                message.MakeTestMessage();

                if (loc != null)
                {
                    message.PositionUpdateRequest.Latitude = loc.Latitude.ToString();
                    message.PositionUpdateRequest.Longitude = loc.Longitude.ToString();
                }
            }

            return View(message);
        }

        private string stringify(object o)
        {
            return new JavaScriptSerializer().Serialize(Json(o).Data);
        }

        [HttpPost]
        public ActionResult Login(TestMessage message)
        {
            var controller = new DeviceController();
            
            var result = controller.Login(message.LoginRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.LoginRequest);
            core_message.LoginRequest = message.LoginRequest;
            core_message.AuthToken = result.AuthToken;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");

        }

        [HttpPost]
        public ActionResult Logout(TestMessage message)
        {
            var controller = new DeviceController();

            message.LogoutRequest.AuthToken = message.AuthToken;
            var result = controller.Logout(message.LogoutRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.LogoutRequest);
            core_message.LogoutRequest = message.LogoutRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CallsignChange(TestMessage message)
        {
            var controller = new DeviceController();

            message.CallsignChangeRequest.AuthToken = message.AuthToken;
            var result = controller.CallsignChange(message.CallsignChangeRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.LogoutRequest);
            core_message.LogoutRequest = message.LogoutRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult GetState(TestMessage message)
        {
            var controller = new DeviceController();

            message.GetAssignedEventRequest.AuthToken = message.AuthToken;
            var result = controller.RefreshState(message.GetAssignedEventRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.GetAssignedEventRequest);
            core_message.GetAssignedEventRequest = message.GetAssignedEventRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AckAssignedEvent(TestMessage message)
        {
            var controller = new DeviceController();

            message.AckAssignedEventRequest.AuthToken = message.AuthToken;
            var result = controller.AckAssignedEvent(message.AckAssignedEventRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.AckAssignedEventRequest);
            core_message.AckAssignedEventRequest = message.AckAssignedEventRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult PositionUpdate(TestMessage message)
        {
            var controller = new DeviceController();

            message.PositionUpdateRequest.Request.AuthToken = message.AuthToken;

            double lat = 0.0, lon = 0.0;
            double.TryParse(message.PositionUpdateRequest.Latitude, out lat);
            double.TryParse(message.PositionUpdateRequest.Longitude, out lon);
            message.PositionUpdateRequest.Request.Vector.Latitude = lat;
            message.PositionUpdateRequest.Request.Vector.Longitude = lon;

            var result = controller.PositionUpdate(message.PositionUpdateRequest.Request);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.PositionUpdateRequest);
            core_message.PositionUpdateRequest = message.PositionUpdateRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult MakePatientObservation(TestMessage message)
        {
            var controller = new DeviceController();

            message.MakePatientObservationRequest.AuthToken = message.AuthToken;
            var result = controller.MakePatientObservation(message.MakePatientObservationRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.MakePatientObservationRequest);
            core_message.MakePatientObservationRequest = message.MakePatientObservationRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult PatientDetails(TestMessage message)
        {
            var controller = new DeviceController();

            message.PatientDetailsRequest.AuthToken = message.AuthToken;
            var result = controller.PatientDetails(message.PatientDetailsRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.PatientDetailsRequest);
            core_message.PatientDetailsRequest = message.PatientDetailsRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SetStatus(TestMessage message)
        {
            var controller = new DeviceController();

            message.SetStatusRequest.AuthToken = message.AuthToken;
            var result = controller.SetStatus(message.SetStatusRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.SetStatusRequest);
            core_message.SetStatusRequest = message.SetStatusRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult GetStatusCodes(TestMessage message)
        {
            var controller = new DeviceController();

            message.GetStatusCodesRequest.AuthToken = message.AuthToken;
            var result = controller.GetStatusCodes(message.GetStatusCodesRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.GetStatusCodesRequest);
            core_message.GetStatusCodesRequest = message.GetStatusCodesRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult GetEntityTypes(TestMessage message)
        {
            var controller = new DeviceController();

            message.GetEntityTypesRequest.AuthToken = message.AuthToken;
            var result = controller.GetEntityTypes(message.GetEntityTypesRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.GetEntityTypesRequest);
            core_message.GetEntityTypesRequest = message.GetEntityTypesRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }



        [HttpPost]
        public ActionResult GetHistory(TestMessage message)
        {
            var controller = new DeviceController();

            message.GetHistoryRequest.AuthToken = message.AuthToken;
            var result = controller.GetHistory(message.GetHistoryRequest);

            var core_message = Session["TestMessage"] as TestMessage;
            core_message.Result = "";
            core_message.Request = "";
            if (result != null)
                core_message.Result = stringify(result);
            else
                core_message.Result = "returned NULL";
            core_message.Request = stringify(message.GetHistoryRequest);
            core_message.GetHistoryRequest = message.GetHistoryRequest;
            Session["TestMessage"] = core_message;

            return RedirectToAction("Index");
        }


    }

}