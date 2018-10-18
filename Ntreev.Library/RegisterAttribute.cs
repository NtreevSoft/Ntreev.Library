#if NETSTANDARD
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Ntreev.Library
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RegisterAttribute : Attribute
    {
        public Type RegistrationType { get; }
        public ServiceLifetime Lifetime { get; } = ServiceLifetime.Singleton;

        public RegisterAttribute(Type registrationType)
        {
            this.RegistrationType = registrationType;
        }

        public RegisterAttribute(Type registrationType, ServiceLifetime serviceLifetime) : this(registrationType)
        {
            this.Lifetime = serviceLifetime;
        }
    }
}
#endif