namespace NServiceBus.Testing.Acceptance.Customization
{
    using System;
    using System.Collections.Generic;

    using NServiceBus.Testing.Acceptance.Support;

    public class Conventions
    {
        static Conventions()
        {
            EndpointNamingConvention = t => "at." + t.Name.Replace("+", "-");
        }

        public static Func<RunDescriptor> DefaultRunDescriptor = () => new RunDescriptor { Key = "Default" };

        public static Func<Type, string> EndpointNamingConvention { get; set; }

        public static Func<IDictionary<string, object>> DefaultDomainData { get; set; }

        public static string DefaultNameFor<T>()
        {
            return EndpointNamingConvention(typeof(T));
        }
    }
}