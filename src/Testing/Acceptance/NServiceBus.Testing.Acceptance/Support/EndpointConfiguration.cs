namespace NServiceBus.Testing.Acceptance.Support
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using NServiceBus;

    public class EndpointConfiguration
    {
        public EndpointConfiguration()
        {
            this.UserDefinedConfigSections = new Dictionary<Type, object>();
            this.TypesToExclude = new List<Type>();
            this.AssembliesToScan = new List<Tuple<Assembly, string>>();
            this.NamespacesToExclude = new List<string>();
            this.PurgeOnStartup = true;
        }

        public IDictionary<Type, Type> EndpointMappings { get; set; }

        public IList<Tuple<Assembly, string>> AssembliesToScan { get; set; } 

        public IList<Type> TypesToExclude { get; set; }

        public IList<string> NamespacesToExclude { get; set; } 

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

        public Func<Type, bool> CommandsDefinition { get; set; }

        public Func<Type, bool> EventsDefinition { get; set; }

        public Func<Type, bool> MessagesDefinition { get; set; }

        public bool PurgeOnStartup { get; set; }
    }
}