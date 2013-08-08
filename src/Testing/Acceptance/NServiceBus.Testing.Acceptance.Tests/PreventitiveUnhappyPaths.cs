namespace NServiceBus.Testing.Acceptance.Tests
{
    using System;
    using System.Linq;

    using NServiceBus.Testing.Acceptance.Customization;
    using NServiceBus.Testing.Acceptance.EndpointTemplates;
    using NServiceBus.Testing.Acceptance.ScenarioDescriptors;
    using NServiceBus.Testing.Acceptance.Support;
    using NServiceBus.Testing.Acceptance.Tests.Commands;

    using NUnit.Framework;

    public class PreventitiveUnhappyPaths
    {
        [SetUp]
        public void SetupConventions()
        {
            Conventions.DefaultRunDescriptor = () => new RunDescriptor(Transports.Msmq, Builders.StructureMap);
        }

        [Test]
        public void ThrowsAggregateExceptionIfMappedTypeNotScanned()
        {
            Assert.Throws<AggregateException>(
                () => Scenario.Define<DefaultContext>()
                          .WithEndpoint<SimpleSendAndReceiveWithSQLiteExample.PizzaService>()
                          .WithEndpoint<WebServer>(
                              builder =>
                              builder.Given(
                                  (bus, context) =>
                                  bus.Send(
                                      new OrderPizzaCommand
                                          {
                                              CustomerName = "Mr Wizard",
                                              PizzaName = "Ham & Cheese Special"
                                          })))
                          .Done(context => context.UnitOfWorkEndedCount == 1)
                          .Should(
                              context =>
                                  {
                                      Assert.AreEqual(0, context.Exceptions.Count());
                                      Assert.AreEqual(1, context.UnitOfWorkEndedCount);
                                  }).Run());
        }

        public class WebServer : EndpointConfigurationBuilder
        {
            public WebServer()
            {
                this.EndpointSetup<DefaultServer>(c => Configure.Features.Disable<Features.SecondLevelRetries>())
                    .ScanningAssembly(typeof(Conventions).Assembly)
                    .DefineCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Tests.Commands"))
                    .AppConfig("NServiceBus.Testing.Acceptance.Tests.dll.config")
                    .AddMapping<DummyCommand>(typeof(SimpleSendAndReceiveWithSQLiteExample.PizzaService))
                    .AddMapping<OrderPizzaCommand>(typeof(SimpleSendAndReceiveWithSQLiteExample.PizzaService));
            }
        }
    }
}