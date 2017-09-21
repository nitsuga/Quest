using Autofac;
using Autofac.Builder;
using Quest.Lib.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Quest.Api
{
    public class AutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Load(builder, Assembly.Load("Quest.Lib"));
            Load(builder, Assembly.Load("Quest.Common"));
        }

        protected void Load(ContainerBuilder builder, Assembly asm)
        {
            foreach (var t in asm.GetTypes())
            {
                RegisterType(builder, t);
            }
        }

        static void RegisterType(ContainerBuilder builder, Type t)
        {

            var attribute = t.CustomAttributes.FirstOrDefault(x => x.AttributeType.GetInterface("IInjectionAttribute") != null);
            if (attribute != null)
            {
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
