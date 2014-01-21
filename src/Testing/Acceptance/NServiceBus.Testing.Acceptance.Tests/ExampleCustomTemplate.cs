namespace NServiceBus.Testing.Acceptance.Tests
{
    using NServiceBus.Config.ConfigurationSource;
    using NServiceBus.Testing.Acceptance.EndpointTemplates;
    using NServiceBus.Testing.Acceptance.Support;

    /// <summary>
    /// Possible to creater custom templates as to replicate exact production fluent config (as can't pass in).
    /// </summary>
    public class ExampleCustomTemplate : IEndpointSetupTemplate
    {
        public Configure GetConfiguration(
            RunDescriptor runDescriptor, EndpointConfiguration endpointConfiguration, IConfigurationSource configSource)
        {
            endpointConfiguration.SetupLogging();

            Configure.Features.Enable<Features.Sagas>();
            Configure.Features.Enable<Features.MsmqTransport>();
            Configure.Features.Disable<Features.SecondLevelRetries>();

            Configure.Serialization.Xml();

            var config = Configure
                .With(AllAssemblies.Matching("NServiceBus.Testing.Acceptance.Tests"))
                .DefineEndpointName(endpointConfiguration.EndpointName)
                .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Tests.Commands"))
                .CustomConfigurationSource(configSource)
                .DefineBuilder(runDescriptor.Settings.GetOrNull("Builder"), runDescriptor.Settings.GetOrNull("BuilderRegistryType"))
                .UseNHibernateSagaPersister()
                .UseNHibernateTimeoutPersister()
                .UseNHibernateSubscriptionPersister()
                .UseTransport<Msmq>()
                .PurgeOnStartup(true)
                .UnicastBus();

            config.Configurer.ConfigureComponent<FailureHandler>(DependencyLifecycle.SingleInstance);
            config.Configurer.ConfigureComponent<UnitOfWorkInterceptor>(DependencyLifecycle.InstancePerCall);

            return config;
        }
    }
}