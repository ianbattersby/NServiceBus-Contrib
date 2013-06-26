namespace NServiceBus.Testing.Acceptance.Support
{
    using System;
    using System.Collections.Generic;

    using NServiceBus;

    public class EndpointConfiguration
    {
        public EndpointConfiguration()
        {
            this.UserDefinedConfigSections = new Dictionary<Type, object>();
            this.TypesToExclude = new List<Type>();
        }

        public IDictionary<Type, Type> EndpointMappings { get; set; }

        public IList<Type> TypesToExclude { get; set; }

        public Func<RunDescriptor, IDictionary<Type, string>, Configure> GetConfiguration { get; set; }

        public string EndpointName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.CustomEndpointName))
                    return this.CustomEndpointName;
                return this.endpointName;
            }
            set { this.endpointName = value; }
        }

        public Type BuilderType { get; set; }

        public string AppConfigPath { get; set; }

        public Address AddressOfAuditQueue { get; set; }

        public IDictionary<Type, object> UserDefinedConfigSections { get; private set; }

        public string CustomMachineName { get; set; }

        public string CustomEndpointName { get; set; }

        string endpointName;
    }
}