using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;

namespace Quest.Lib.Utils
{
    public class MEF
    {

        public enum Method
        {
            Directory,
            Domain,
            Application
        }

        string[] _parts;

        /// <summary>
        /// Create a container for all injectable objects
        /// ProcessorModules are filtered to those specified in the Modules list
        /// IOptionalComponents are filtered to those specified in the Parts list
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="exceptionManager"></param>
        /// <param name="parts">only loads MEF components derived from IOptionalComponent that are in this list</param>
        /// <param name="method"></param>
        /// <returns></returns>
        public CompositionContainer InitialiseMef(object owner, ExceptionManager exceptionManager, string[] parts, Method method)
        {
            _parts = parts;
            CompositionContainer container = new CompositionContainer();
            Logger.Write($"Constructing Composition Container", GetType().Name);
            try
            {
                ////An aggregate catalog that combines multiple catalogs
                var catalog = new AggregateCatalog();

                switch (method)
                {
                    case Method.Domain:
                        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            try
                            {
                                Debug.Write($".. loading parts in {a.FullName}");
                                var assemblycatalog = new AssemblyCatalog(a);
                                if (assemblycatalog.Parts.Any())
                                {
                                    var fcat = new FilteredCatalog(assemblycatalog, PartFilter);
                                    catalog.Catalogs.Add(fcat);
                                }
                            }
                            catch
                            {
                            }
                        }
                        break;

                   case   Method.Application:
                        try
                        {

                            //Debug.Write($".. loading parts in {a.FullName}");
                            var assemblycatalog = new ApplicationCatalog();
                            if (assemblycatalog.Parts.Any())
                            {
                                var fcat = new FilteredCatalog(assemblycatalog, PartFilter);
                                catalog.Catalogs.Add(fcat);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Write($"{ex}");
                        }
                        break;

                    case Method.Directory:
                        var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll",
                            SearchOption.AllDirectories);

                        foreach (var file in files)
                        {
                            try
                            {
                                Debug.Write($".. loading parts in {file}");
                                var assemblycatalog = new AssemblyCatalog(file);
                                if (assemblycatalog.Parts.Any())
                                {
                                    var fcat = new FilteredCatalog(assemblycatalog, PartFilter);
                                    catalog.Catalogs.Add(fcat);
                                }
                            }
                            catch
                            {
                            }
                        }
                        break;
                }

                container = new CompositionContainer(catalog);
                container.ComposeExportedValue(exceptionManager);
                container.ComposeExportedValue(container);

                var msgHandler = container.GetExports<MessageHandler>();

            }
            catch (Exception ex)
            {
                Logger.Write($"MEF failure: {ex} ", GetType().Name);
            }

            if (container == null) throw new ArgumentNullException(nameof(container));

            //Fill the imports of this object
            if (owner != null)
            {
                try
                {
                    container.ComposeParts(owner);
                }
                catch (CompositionException compositionException)
                {
                    Logger.Write($"Composition failure: {compositionException} ", GetType().Name);
                }
            }

            return container;
        }



        /// <summary>
        /// filter only the ProcessorModules and IOptionalComponent we want
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        bool PartFilter(ComposablePartDefinition definition)
        {
            foreach (var d in definition.ExportDefinitions)
            {
                var type = d.Metadata["ExportTypeIdentity"].ToString();
                if (type == "Quest.Lib.Utils.IOptionalComponent")
                {
                    string displayName = ((ICompositionElement)definition).DisplayName;
                    bool partWanted = _parts.Contains(displayName);
                    Logger.Write($"found part {displayName} - {(partWanted ? "Required" : "Not required")}", GetType().Name);
                    return partWanted;
                }
            }
            Logger.Write($"found export {definition}", GetType().Name);

            return true;
        }

        public static void StartParts(CompositionContainer container, ExceptionManager exceptionManager)
        {
            try
            {
                Logger.Write("Starting parts", TraceEventType.Information, "MEF");
                var parts = container.GetExports<IOptionalComponent>();
                foreach (var part in parts)
                {
                    try
                    {
                        var instance = part.Value;
                        var name = instance.GetType().Name;
                        Logger.Write($"Initialising part {name}","MEF");

                        var sim = instance as IPart;
                        sim?.Initialise();
                    }
                    catch (Exception ex)
                    {
                        if (exceptionManager != null && exceptionManager.HandleException(ex, "TracePolicy"))
                            throw;
                    }
                }
            }
            catch (Exception ex)
            {
                if (exceptionManager != null && exceptionManager.HandleException(ex, "TracePolicy"))
                    throw;
            }
        }

        public static void AutoStartJobs(StringCollection instances, JobManager jobManager, ExceptionManager exceptionManager)
        {
            try
            {
                Logger.Write("Starting jobs", TraceEventType.Information, "Web");
                foreach (var instance in instances)
                {
                    try
                    {
                        Logger.Write("Starting " + instance,"MEF");

                        if (instance.StartsWith("#"))
                            continue;

                        jobManager.AddJobFromTemplate(instance);
                    }
                    catch (Exception ex)
                    {
                        if (exceptionManager != null && exceptionManager.HandleException(ex, "TracePolicy"))
                            throw;
                    }
                }
            }
            catch (Exception ex)
            {
                if (exceptionManager != null && exceptionManager.HandleException(ex, "TracePolicy"))
                    throw;
            }
        }
    }

    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
        private readonly AggregateCatalog _catalog;

        public SafeDirectoryCatalog(string directory)
        {
            var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);

            _catalog = new AggregateCatalog();

            foreach (var file in files)
            {
                try
                {
                    var asmCat = new AssemblyCatalog(file);

                    //Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (asmCat.Parts.ToList().Count > 0)
                        _catalog.Catalogs.Add(asmCat);
                }
                catch (ReflectionTypeLoadException)
                {
                }
                catch (BadImageFormatException)
                {
                }
            }
        }
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return _catalog.Parts; }
        }
    }
}