namespace NServiceBus.Testing.Acceptance.Support
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using NServiceBus.Config;
    using NServiceBus.Config.ConfigurationSource;

    public class ScenarioConfigSource : IConfigurationSource
    {
        readonly EndpointConfiguration configuration;
        readonly IDictionary<Type, string> routingTable;

        public ScenarioConfigSource(EndpointConfiguration configuration, IDictionary<Type, string> routingTable)
        {
            this.configuration = configuration;
            this.routingTable = routingTable;
        }

        public T GetConfiguration<T>() where T : class, new()
        {
            var type = typeof (T);

            if (this.configuration.UserDefinedConfigSections.ContainsKey(type))
                return this.configuration.UserDefinedConfigSections[type] as T;


            if (type == typeof (MessageForwardingInCaseOfFaultConfig))
                return new MessageForwardingInCaseOfFaultConfig
                    {
                        ErrorQueue = "error"
                    } as T;
            
            if (type == typeof(UnicastBusConfig))
                return new UnicastBusConfig
                    {
                        ForwardReceivedMessagesTo = this.configuration.AddressOfAuditQueue != null ? this.configuration.AddressOfAuditQueue.ToString() : null,
                        MessageEndpointMappings = this.GenerateMappings()
                    }as T;



            if (type == typeof(Logging))
                return new Logging()
                {
                    Threshold = "WARN"
                } as T;


            return ConfigurationManager.GetSection(type.Name) as T;
        }

        MessageEndpointMappingCollection GenerateMappings()
        {
            var mappings = new MessageEndpointMappingCollection();

            foreach (var templateMapping in this.configuration.EndpointMappings)
            {
                var messageType = templateMapping.Key;
                var endpoint = templateMapping.Value;

               mappings.Add( new MessageEndpointMapping
                    {
                        AssemblyName = messageType.Assembly.FullName,
                        TypeFullName = messageType.FullName,
                        Endpoint = this.routingTable[endpoint]
                    });
            }

            return mappings;
        }
    }
}