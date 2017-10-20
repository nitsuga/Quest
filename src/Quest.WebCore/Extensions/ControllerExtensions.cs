using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.IO;

namespace Quest.WebCore.Extensions
{
    public static class ControllerExtensions
    {
        public static string PartialViewToString(this Controller controller)
        {
            return controller.RenderPartialViewToString(null, null);
        }

        public static string RenderPartialViewToString(this Controller controller, string viewName)
        {
            return controller.RenderPartialViewToString(viewName, null);
        }

        public static string RenderPartialViewToString(this Controller controller, object model)
        {
            return controller.RenderPartialViewToString(null, model);
        }

        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            //if (string.IsNullOrEmpty(viewName))
           // {
                //viewName = controller.ControllerContext.RouteData.GetRequiredString("action");
            //    viewName = controller.ControllerContext.RouteData.GetRequiredString("action");
            //}

            controller.ViewData.Model = model;

            ViewEngineResult viewResult = Microsoft.AspNetCore.Mvc.ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
            ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData);
            viewResult.View.RenderAsync(viewContext);

            using (StringWriter stringWriter = new StringWriter())
            {
                return stringWriter.GetStringBuilder().ToString();
            }
        }

    }
}