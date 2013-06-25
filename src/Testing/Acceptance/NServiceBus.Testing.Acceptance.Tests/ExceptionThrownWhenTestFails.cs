namespace NServiceBus.Testing.Acceptance.Tests
{
    using System;
    using System.Linq;

    using NServiceBus.Config;
    using NServiceBus.Testing.Acceptance.Customization;
    using NServiceBus.Testing.Acceptance.EndpointTemplates;
    using NServiceBus.Testing.Acceptance.ScenarioDescriptors;
    using NServiceBus.Testing.Acceptance.Support;

    using NUnit.Framework;

    public class CustomContext : DefaultContext
    {
        public bool HandlerInvoked { get; set; }

        public DateTime? HandledAt { get; set; }
    }

    public class ExceptionThrownWhenTestFails
    {
        [SetUp]
        public void SetupConventions()
        {
            Conventions.DefaultRunDescriptor = () => new RunDescriptor(Transports.Msmq, Builders.StructureMap);
        }

        [Test]
        public void DetectsExceptionAndReports()
        {
            Scenario.Define<CustomContext>()
                    .WithEndpoint<Publisher>(
                        builder =>
                            {
                                builder.MonitorSubscriptions();

                                builder.When(context => context.SubscriptionsCount == 1, (bus, context) =>
                                    bus.Publish(
                                        new TestEvent
                                            {
                                                Something = "Hello!"
                                            }));
                            })
                    .WithEndpoint<Subscriber>(builder => builder.Subscribe<TestEvent>())
                    .Done(context => context.SubscriptionsCount == 1 && context.HandlerInvoked)
                    .Should(context =>
                        {
                            Assert.AreEqual(1, context.Exceptions.Count());
                        })
                    .Run();
        }

        [Test]
        public void AbortsATestRunIfExceptionDetected()
        {
            Scenario.Define<CustomContext>()
                    .WithEndpoint<Publisher>(
                        builder =>
                        {
                            builder.MonitorSubscriptions();

                            builder.When(context => context.SubscriptionsCount == 1, (bus, context) =>
                                bus.Publish(
                                    new TestEvent
                                    {
                                        Something = "Hello!"
                                    }));
                        })
                    .WithEndpoint<Subscriber>(builder => builder.Subscribe<TestEvent>())
                    .Done(context => context.SubscriptionsCount == 1 && context.HandlerInvoked & 1 == 2)
                    .Should(context =>
                    {
                        Assert.NotNull(context.HandledAt);
                        Assert.Greater(30, DateTime.UtcNow.Subtract(((DateTime)context.HandledAt)).Seconds);
                    })
                    .Run();
        }

        public class Subscriber : EndpointConfigurationBuilder
        {
            public Subscriber()
            {
                this.EndpointSetup<DefaultServer>(c =>
                    {
                        Configure.Features.Disable<Features.SecondLevelRetries>();
                        Configure.Features.Disable<Features.AutoSubscribe>();
                    })
                    .ScanningAssembly(typeof(TestEvent).Assembly)
                    .DefineEventsAs(t => t.Namespace != null && t.Name.EndsWith("TestEvent"))
                    .AppConfig("NServiceBus.Testing.Acceptance.Tests.dll.config")
                    .AddMapping<TestEvent>(typeof(Publisher))
                    .WithConfig<TransportConfig>(c => c.MaxRetries = 0);
            }
        }

        public class Publisher : EndpointConfigurationBuilder
        {
            public Publisher()
            {
                this.EndpointSetup<DefaultServer>(c => Configure.Features.Disable<Features.SecondLevelRetries>())
                    .ScanningAssembly(typeof(TestEvent).Assembly)
                    .DefineEventsAs(t => t.Namespace != null && t.Name.EndsWith("TestEvent"))
                    .AppConfig("NServiceBus.Testing.Acceptance.Tests.dll.config")
                    .WithConfig<TransportConfig>(c => c.MaxRetries = 0);
            }
        }
    }

    public class TestEventHandler : IHandleMessages<TestEvent>
    {
        public CustomContext CustomContext { get; set; }

        public void Handle(TestEvent message)
        {
            this.CustomContext.HandlerInvoked = true;
            this.CustomContext.HandledAt = DateTime.UtcNow;
            throw new Exception("This is broke on purpose!");
        }
    }

    public class TestEvent
    {
        public string Something { get; set; }
    }
}