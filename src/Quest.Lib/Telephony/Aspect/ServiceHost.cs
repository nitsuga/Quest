using System;
using System.ServiceModel;

namespace Quest.Lib.Telephony.AspectCTIPS
{
    /// <summary>
    /// https://github.com/DigDes/SoapCore/tree/master/src/SoapCore
    /// </summary>
    internal class ServiceHost
    {
        public ServiceHost(CTIEventHandler handler, Uri uri)
        { }

        public void AddServiceEndpoint(Type tp, BasicHttpBinding binding, Uri uri)
        {
        }

        public void Open()
        { }

        public void Close()
        { }

        public void Abort()
        { }
    }
}