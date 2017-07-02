using System.Web.Optimization;

namespace Quest.Mobile
{
    public class BundleConfig
    {        
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/settings").Include(
                "~/js/settings.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-3.1.1.js",
                "~/Scripts/jquery.cookie.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Scripts/bootbox.js",
                "~/Scripts/bootstrap-toggle.js",
                "~/Scripts/jquery.pnotify.js",
                "~/Scripts/bootstrap.js",
                "~/js/clientstore.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive.js"
                ));

            bundles.Add(new StyleBundle("~/Content/core").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-theme.css",
                "~/Content/jquery.pnotify.default.css",
                "~/Content/jquery.pnotify.default.icons.css",
                "~/Content/normalize.css",
                "~/Content/bootstrap-toggle.less",
                "~/Content/font-awesome.min.css",
                "~/css/jquery-ui-1.12.0.custom/jquery-ui.css",
                "~/css/jquery-ui-1.12.0.custom/jquery-ui.theme.css",
                "~/css/jquery-ui-1.12.0.custom/jquery-ui.structure.css",
                "~/css/homepage.css"
                ));

            // if lte IE 8]>
            bundles.Add(new ScriptBundle("~/bundles/ie8").Include(
                "~/js/aight.js",
                "~/js/aight.d3.js"
                ));

           bundles.Add(new StyleBundle("~/Content/map").Include(
                "~/Content/jquery.splitter.css",
                "~/Content/leaflet.css",
                "~/css/leaflet.usermarker.css",
                "~/css/leaflet.draw.css",
                "~/css/leaflet.Icon.Pulse.css",
                "~/css/map.css",
                 "~/css/vis/vis.css"
                ));

            bundles.Add(new ScriptBundle("~/bundles/map").Include(
                "~/js/href.js",      // use latest candidate release
                "~/js/leaflet-src.js",      // use latest candidate release
                "~/js/proj4-compressed.js",
                "~/js/proj4leaflet.js",
                "~/js/leaflet.markercluster.js",
                "~/js/leaflet.draw.js",
                "~/js/leaflet.usermarker.js",
                "~/js/leaflet.spin.js",
                "~/js/leaflet.heat.js",
                "~/js/leaflet.Icon.Pulse.js",
                "~/js/leaflet.playback.js",
                "~/js/spin.js",
                "~/js/wicket.js",
                "~/js/wicket-leaflet.js",
                "~/js/Google.js",
                "~/js/telephony.js",
                "~/js/timeline.js",
                "~/js/intel.js",
                "~/js/vis.min.js",
                //"~/js/vis-timeline-graph2d.min.js",
                "~/js/quest.visualisation.js",
                "~/js/map.js"
            ));
            
            bundles.Add(new ScriptBundle("~/bundles/jobs").Include(
                "~/js/jobs.js"
            ));


            bundles.Add(new ScriptBundle("~/bundles/security").Include(
                "~/js/vis.min.js",
                "~/js/security.js"
            ));


            bundles.Add(new ScriptBundle("~/bundles/testlocations").Include(
                "~/js/testlocations.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/testroutes").Include(
                "~/js/testroutes.js"
            ));

            //bundles.Add(new ScriptBundle("~/bundles/heldcallshistory").Include(
            //    "~/js/d3-v3.js",
            //    "~/js/nv.d3.js",
            //    "~/js/heldcallshistory.js"
            //));

            //bundles.Add(new StyleBundle("~/Content/heldcallshistory").Include(
            //    "~/css/dc.css",
            //    "~/css/nv.d3.css",
            //    "~/css/heldcallshistory.css"
            //));


            //bundles.Add(new ScriptBundle("~/bundles/heldcallssummary").Include(
            //    "~/js/d3-v3.js",
            //    "~/js/nv.d3.js",
            //    "~/js/crossfilter.js",
            //    "~/js/dc.js",
            //    "~/js/heldcallssummary.js"
            //));

            //bundles.Add(new StyleBundle("~/Content/heldcallssummary").Include(
            //    "~/css/nv.d3.css",
            //    "~/css/dc.css",
            //    "~/css/heldcallssummary.css"
            //));


            //bundles.Add(new ScriptBundle("~/bundles/ccghistory").Include(
            //    "~/js/d3-v3.js",
            //    "~/js/nv.d3.js",
            //    "~/js/ccgshistory.js"
            //));

            //bundles.Add(new StyleBundle("~/Content/ccgsummary").Include(
            //    "~/css/dc.css",
            //    "~/css/nv.d3.css",
            //    "~/css/ccgsummary.css"
            //));


            //bundles.Add(new ScriptBundle("~/bundles/ccghistory").Include(
            //    "~/js/d3-v3.js",
            //    "~/js/nv.d3.js",
            //    "~/js/heldcallshistory.js"
            //));

            //bundles.Add(new StyleBundle("~/Content/ccghistory").Include(
            //    "~/css/dc.css",
            //    "~/css/nv.d3.css",
            //    "~/css/heldcallshistory.css"
            //));


            //bundles.Add(new ScriptBundle("~/bundles/dashboard").Include(
            //    "~/js/dashboard.js"
            //));
            //bundles.Add(new StyleBundle("~/Content/dashboard").Include(
            //    "~/css/dc.css"
            //));

            //bundles.Add(new ScriptBundle("~/bundles/crossfilter").Include(
            //    "~/js/d3/d3.js",
            //    "~/js/dc/dc.js",
            //    "~/js/crossfilter/crossfilter.js"
            //    ));


        }
    }
}