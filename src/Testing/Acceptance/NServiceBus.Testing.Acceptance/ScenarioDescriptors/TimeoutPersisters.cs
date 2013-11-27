namespace NServiceBus.Testing.Acceptance.ScenarioDescriptors
{
    using System.Collections.Generic;

    using NServiceBus.Persistence.InMemory.TimeoutPersister;
    using NServiceBus.Persistence.Raven.TimeoutPersister;
    using NServiceBus.TimeoutPersisters.NHibernate;

    using Support;

    public static class TimeoutPersisters
    {
        public static readonly RunDescriptor InMemory = new RunDescriptor
            {
                Key = "InMemoryTimeout",
                Settings =
                    new Dictionary<string, string>
                        {
                            {
                                "TimeoutPersister",
                                typeof(InMemoryTimeoutPersistence).AssemblyQualifiedName
                            }
                        }
            };

        public static readonly RunDescriptor Raven = new RunDescriptor
                                                         {
                                                             Key = "RavenTimeout",
                                                             Settings =
                                                                new Dictionary<string, string>
                                                                {
                                                                    {
                                                                    "TimeoutPersister",
                                                                    typeof(RavenTimeoutPersistence).AssemblyQualifiedName
                                                                }
                                                             }
                                                         };

        public static readonly RunDescriptor NHibernate = new RunDescriptor
        {
            Key = "NHTimeout",
            Settings =
                new Dictionary<string, string>
                        {
                            {
                                "TimeoutPersister",
                                typeof(TimeoutStorage).AssemblyQualifiedName
                            }
                        }
        };
    }
}