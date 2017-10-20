using Autofac;
using Autofac.Builder;
using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Quest.Lib.DependencyInjection
{
    /// <summary>
    /// Register components marked with an Injection attribute
    /// </summary>
    public class AutofacModule : Autofac.Module
    {
        IEnumerable<string> _assemblies;


        public AutofacModule(IEnumerable<string> assemblies)
        {
            _assemblies = assemblies;
        }

        protected override void Load(ContainerBuilder builder)
        {
            foreach (var asm in _assemblies)
            {
                Logger.Write($"Scanning assembly {asm} for components");
                Load(builder, Assembly.Load(asm));
            }
        }

        protected void Load(ContainerBuilder builder, Assembly asm)
        {
            int count = 0;
            foreach (var t in asm.GetTypes())
                count+=RegisterType(builder, t);
            Logger.Write($".. registered {count} types from {asm.FullName}");
        }

        static int RegisterType(ContainerBuilder builder, Type t)
        {
            int count = 0;
            var attribute = t.CustomAttributes.FirstOrDefault(x => x.AttributeType.GetInterface("IInjectionAttribute") != null);
            if (attribute != null)
            {
                Logger.Write($".. registering type {t.FullName}");
                count++;
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
                    {
                        if (name != null)
                        {
                            b = b.Named(name, theinterface);
                        }
                        b = b.As(theinterface);
                    }
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
            return count;
        }
    }
}
