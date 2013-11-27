namespace NServiceBus.Testing.Acceptance.ScenarioDescriptors
{
    using Support;

    public class AllTimeoutPersisters : ScenarioDescriptor
    {
        public AllTimeoutPersisters()
        {
            Add(TimeoutPersisters.InMemory);
            Add(TimeoutPersisters.Raven);
            Add(TimeoutPersisters.NHibernate);
        }
    }
}