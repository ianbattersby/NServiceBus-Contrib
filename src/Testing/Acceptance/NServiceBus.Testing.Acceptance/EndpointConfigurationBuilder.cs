namespace NServiceBus.Testing.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Support;

    public class EndpointConfigurationBuilder : IEndpointConfigurationFactory
    {
        public EndpointConfigurationBuilder()
        {
            this.configuration.EndpointMappings = new Dictionary<Type, Type>();
        }

        public EndpointConfigurationBuilder AuditTo(Address addressOfAuditQueue)
        {
            this.configuration.AddressOfAuditQueue = addressOfAuditQueue;

            return this;
        }

        public EndpointConfigurationBuilder CustomMachineName(string customMachineName)
        {
            this.configuration.CustomMachineName = customMachineName;

            return this;
        }

        public EndpointConfigurationBuilder CustomEndpointName(string customEndpointName)
        {
            this.configuration.CustomEndpointName = customEndpointName;

            return this;
        }


        public EndpointConfigurationBuilder AppConfig(string path)
        {
            this.configuration.AppConfigPath = path;

            return this;
        }


        public EndpointConfigurationBuilder AddMapping<T>(Type endpoint)
        {
            this.configuration.EndpointMappings.Add(typeof(T), endpoint);

            return this;
        }

        EndpointConfiguration CreateScenario()
        {
            this.configuration.BuilderType = this.GetType();

            return this.configuration;
        }

        public EndpointConfigurationBuilder EndpointSetup<T>() where T : IEndpointSetupTemplate
        {
            return this.EndpointSetup<T>(c => { });
        }

        public EndpointConfigurationBuilder EndpointSetup<T>(Action<Configure> configCustomization) where T : IEndpointSetupTemplate
        {
            this.configuration.GetConfiguration = (settings, routingTable) =>
            {
                var config = ((IEndpointSetupTemplate)Activator.CreateInstance<T>()).GetConfiguration(settings, this.configuration, new ScenarioConfigSource(this.configuration, routingTable));

                configCustomization(config);

                return config;
            };

            return this;
        }

        EndpointConfiguration IEndpointConfigurationFactory.Get()
        {
            return this.CreateScenario();
        }

        public class SubscriptionsSpy : IAuthorizeSubscriptions
        {
            private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            private int subscriptionsReceived;

            public int NumberOfSubscriptionsToWaitFor { get; set; }

            public bool AuthorizeSubscribe(string messageType, string clientEndpoint,
                                           IDictionary<string, string> headers)
            {
                if (Interlocked.Increment(ref this.subscriptionsReceived) >= this.NumberOfSubscriptionsToWaitFor)
                {
                    this.manualResetEvent.Set();
                }

                return true;
            }

            public bool AuthorizeUnsubscribe(string messageType, string clientEndpoint,
                                             IDictionary<string, string> headers)
            {
                return true;
            }

            public void Wait()
            {
                if (!this.manualResetEvent.WaitOne(TimeSpan.FromSeconds(20)))
                    throw new Exception("No subscription message was received");

            }
        }


        readonly EndpointConfiguration configuration = new EndpointConfiguration();

        public EndpointConfigurationBuilder WithConfig<T>(Action<T> action)
        {
            var config = Activator.CreateInstance<T>();

            action(config);

            this.configuration.UserDefinedConfigSections[typeof(T)] = config;

            return this;
        }

        public EndpointConfigurationBuilder ExcludeType<T>()
        {
            this.configuration.TypesToExclude.Add(typeof(T));

            return this;
        }
    }
}