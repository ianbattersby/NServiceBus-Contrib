namespace NServiceBus.Testing.Acceptance.ScenarioDescriptors
{
    using System.Collections.Generic;

    using NServiceBus.Persistence.InMemory.SubscriptionStorage;
    using NServiceBus.Persistence.Msmq.SubscriptionStorage;
    using NServiceBus.Persistence.Raven.SubscriptionStorage;
    using NServiceBus.Unicast.Subscriptions.NHibernate;

    using Support;

    public static class SubscriptionStorages
    {
        public static readonly RunDescriptor InMemory = new RunDescriptor
            {
                Key = "InMemorySub",
                Settings =
                    new Dictionary<string, string>
                        {
                            {
                                "SubscriptionStorage",
                                typeof (InMemorySubscriptionStorage).AssemblyQualifiedName
                            }
                        }
            };


        public static readonly RunDescriptor Raven = new RunDescriptor
            {
                Key = "RavenSub",
                Settings =
                    new Dictionary<string, string>
                        {
                            {
                                "SubscriptionStorage",
                                typeof (RavenSubscriptionStorage).AssemblyQualifiedName
                            }
                        }
            };

        public static readonly RunDescriptor NHibernate = new RunDescriptor
            {
                Key = "NHSub",
                Settings =
                    new Dictionary<string, string>
                        {
                            {
                                "SubscriptionStorage",
                                typeof (SubscriptionStorage).AssemblyQualifiedName
                            }
                        }
            };

        public static readonly RunDescriptor Msmq = new RunDescriptor
            {
                Key = "MsmqSub",
                Settings =
                    new Dictionary<string, string>
                        {
                            {
                                "SubscriptionStorage",
                                typeof (MsmqSubscriptionStorage).AssemblyQualifiedName
                            }
                        }
            };
    }
}