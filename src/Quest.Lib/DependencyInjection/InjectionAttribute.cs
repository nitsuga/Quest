using System;

namespace Quest.Lib.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InjectionAttribute : Attribute, IInjectionAttribute
    {
        public InjectionAttribute()
        {
        }

        public InjectionAttribute(Lifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public InjectionAttribute(string name)
        {
            Name = name;
        }

        public InjectionAttribute(Type contractType)
        {
            ContractType = contractType;
        }

        public InjectionAttribute(string name, Type contractType)
        {
            Name = name;
            ContractType = contractType;
        }

        public InjectionAttribute(string name, Lifetime lifetime)
        {
            Name = name;
            Lifetime = lifetime;
        }

        public InjectionAttribute(Type contractType, Lifetime lifetime)
        {
            ContractType = contractType;
            Lifetime = lifetime;
        }

        public InjectionAttribute(string name, Type contractType, Lifetime lifetime)
        {
            Name = name;
            ContractType = contractType;
            Lifetime = lifetime;
        }

        private Lifetime Lifetime { get; set; } = Lifetime.PerDependency;

        private Type ContractType { get; set; } = null;

        private string Name { get; set; } = null;
    }
}
