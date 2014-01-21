namespace NServiceBus.Testing.Acceptance.Tests
{
    using System;
    using System.Linq;

    using NServiceBus.Config;
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
            Scenario.Define<ComplexContext>()
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
                        Assert.AreEqual("Ian Was Ere", context.SomeString);
                        Assert.AreEqual(new Guid("AE60893B-4B57-44D1-BDE7-6AA805D0C7AF"), context.SomeGuid);
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

    public class ComplexContext : DefaultContext
    {
        public Guid SomeGuid { get; set; }

        public string SomeString { get; set; }
    }
}