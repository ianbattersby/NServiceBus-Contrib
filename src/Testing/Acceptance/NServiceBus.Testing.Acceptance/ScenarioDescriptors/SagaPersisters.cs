﻿namespace NServiceBus.Testing.Acceptance.ScenarioDescriptors
{
    using System.Collections.Generic;

    using NServiceBus.Persistence.InMemory.SagaPersister;
    using NServiceBus.Persistence.Raven.SagaPersister;
    using NServiceBus.SagaPersisters.NHibernate;
    
    using Support;

    public static class SagaPersisters
    {
        public static readonly RunDescriptor InMemory = new RunDescriptor
            {
                Key = "InMemorySagaPersister",
                Settings =
                    new Dictionary<string, string>
                        {
                            {
                                "SagaPersister",
                                typeof (InMemorySagaPersister).AssemblyQualifiedName
                            }
                        }
            };


        public static readonly RunDescriptor Raven = new RunDescriptor
            {
                Key = "RavenSagaPersister",
                Settings =
                    new Dictionary<string, string>
                        {
                            {
                                "SagaPersister",
                                typeof (RavenSagaPersister).AssemblyQualifiedName
                            }
                        }
            };

        public static readonly RunDescriptor NHibernate = new RunDescriptor
        {
            Key = "NHibernateSagaPersister",
            Settings =
                new Dictionary<string, string>
                        {
                            {
                                "SagaPersister",
                                typeof (SagaPersister).AssemblyQualifiedName
                            }
                        }
        };
    }
}