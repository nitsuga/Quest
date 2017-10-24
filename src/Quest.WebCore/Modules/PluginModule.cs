using Autofac;
using Autofac.Builder;
using Quest.Lib.DependencyInjection;
using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Quest.WebCore.Modules
{
    internal class PluginModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = GetPluginAssemblies();

            foreach( var ass in assemblies)
                Load(builder, ass);
            Load(builder, Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Retrieves all the plugin dlls in the Plugins folder
        /// </summary>
        /// <returns></returns>
        public static List<Assembly> GetPluginAssemblies()
        {
            //TODO: this code needs to search recursively for dlls in the WebRootPath
#if false

            // Identify the location of the Plugins folder
            var pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

            var pluginAssemblies =
                from file in new DirectoryInfo(pluginDirectory).GetFiles()
                where file.Extension.ToLower() == ".dll"
                select Assembly.Load(AssemblyName.GetAssemblyName(file.FullName));

            return pluginAssemblies.ToList();
#endif
            return new List<Assembly>();
        }


        private void Load(ContainerBuilder builder, Assembly asm)
        {
            foreach (var t in asm.GetTypes())
            {
                RegisterType(builder, t);
            }
        }

        private static void RegisterType(ContainerBuilder builder, Type t)
        {
            var attribute = t.CustomAttributes.FirstOrDefault(x => x.AttributeType.GetInterface("IInjectionAttribute") != null);
            if (attribute != null)
            {
                Logger.Write($"Registering {t.Name}");
                var data = CustomAttributeData.GetCustomAttributes(t);

                Type theinterface = null;
                string name = null;
                Lifetime thelifetime = Lifetime.PerDependency;

                foreach (var d in data)
                {
                    foreach (var ca in d.ConstructorArguments)
                    {
                        if (ca.ArgumentType == typeof(string))
                        {
                            name = ca.Value as string;
                        }
                        if (ca.ArgumentType == typeof(Lifetime))
                        {
                            thelifetime = (Lifetime)ca.Value;
                        }
                        if (ca.ArgumentType == typeof(Type))
                        {
                            theinterface = ca.Value as Type;
                        }
                    }
                    IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> b;

                    b = builder.RegisterType(t)
                        .PropertiesAutowired();

                    if (theinterface != null)
                        if (name != null)
                            b = b.Named(name, theinterface);
                        else
                            b = b.As(theinterface);
                    else
                        if (name != null)
                        b = b.Named(name, attribute.AttributeType);


                    switch (thelifetime)
                    {
                        case Lifetime.Singleton:
                            b = b.SingleInstance();
                            break;
                        case Lifetime.PerDependency:
                            b = b.InstancePerDependency();
                            break;
                        case Lifetime.PerLifetime:
                            b = b.InstancePerLifetimeScope();
                            break;
                    }
                }
            }
        }
    }
}
