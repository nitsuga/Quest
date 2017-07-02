#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Web.Http.Dependencies;

namespace Quest.Mobile.Injection
{
    /// <summary>
    /// Resolve dependencies for MVC / Web API using MEF.
    /// </summary>
    public class MefDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver, System.Web.Mvc.IDependencyResolver
    {
        private readonly CompositionContainer _container;

        public MefDependencyResolver(CompositionContainer container)
        {
            _container = container;
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        /// <summary>
        /// Called to request a service implementation.
        /// 
        /// Here we call upon MEF to instantiate implementations of dependencies.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>Service implementation or null.</returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            var name = AttributedModelServices.GetContractName(serviceType);
            var export = _container.GetExportedValueOrDefault<object>(name);
            return export;
        }

        /// <summary>
        /// Called to request service implementations.
        /// 
        /// Here we call upon MEF to instantiate implementations of dependencies.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>Service implementations.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            try
            {
                var exports = _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
                return exports;
            }
                // ReSharper disable once UnusedVariable
            catch (Exception)
            {
                // ignored
            }
            return null;
        }

        public void Dispose()
        {
        }
    }
}

