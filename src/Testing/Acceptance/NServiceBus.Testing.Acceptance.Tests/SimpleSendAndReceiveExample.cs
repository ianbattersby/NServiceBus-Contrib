namespace NServiceBus.Testing.Acceptance.Tests
{
    using System.Linq;

    using NServiceBus.Config;
    using NServiceBus.Config.ConfigurationSource;
    using NServiceBus.Testing.Acceptance.Customization;
    using NServiceBus.Testing.Acceptance.EndpointTemplates;
    using NServiceBus.Testing.Acceptance.ScenarioDescriptors;
    using NServiceBus.Testing.Acceptance.Support;
    using NServiceBus.Testing.Acceptance.Tests.Commands;

    using NUnit.Framework;

    public class SimpleSendAndReceiveWithSQLiteExample
    {
        [SetUp]
        public void SetupConventions()
        {
            Conventions.DefaultRunDescriptor = () => new RunDescriptor(Transports.Msmq, Builders.StructureMap);
        }

        [Test]
        public void CanSendValidCommand()
        {
            Scenario.Define<DefaultContext>()
                .WithEndpoint<PizzaService>()
                .WithEndpoint<WebServer>(builder =>
                    builder.Given((bus, context) => bus.Send(
                        new OrderPizzaCommand
                          {
                              CustomerName = "Mr Wizard",
                              PizzaName = "Ham & Cheese Special"
                          })))
                .Done(context => context.UnitOfWorkEndedCount == 1)  // Meh, not nice but won't pollute production with 'Context'.
                .Should(context =>
                    {
                        Assert.AreEqual(0, context.Exceptions.Count());
                        Assert.AreEqual(1, context.UnitOfWorkEndedCount);
                    })
                .Run();
        }

        public class WebServer : EndpointConfigurationBuilder
        {
            public WebServer()
            {
                this.EndpointSetup<DefaultServer>(c => Configure.Features.Disable<Features.SecondLevelRetries>())
                    .ScanningAssembly(typeof(OrderPizzaCommand).Assembly)
                    .DefineCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Tests.Commands"))
                    .AppConfig("NServiceBus.Testing.Acceptance.Tests.dll.config")
                    .AddMapping<OrderPizzaCommand>(typeof(PizzaService))
                    .WithConfig<TransportConfig>(c => c.MaxRetries = 0);
            }
        }

        public class PizzaService : EndpointConfigurationBuilder
        {
            public PizzaService()
            {
                this.EndpointSetup<ExampleCustomTemplate>()
                    .AppConfig("NServiceBus.Testing.Acceptance.Tests.dll.config")
                    .WithConfig<TransportConfig>(c => c.MaxRetries = 0);
            }
        }
    }

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
                    .UseNHibernateGatewayPersister()
                    .UseTransport<Msmq>()
                    .PurgeOnStartup(true)
                    .UnicastBus();

            config.Configurer.ConfigureComponent<FailureHandler>(DependencyLifecycle.SingleInstance);
            config.Configurer.ConfigureComponent<UnitOfWorkInterceptor>(DependencyLifecycle.InstancePerCall);

            return config;
        }
    }
}