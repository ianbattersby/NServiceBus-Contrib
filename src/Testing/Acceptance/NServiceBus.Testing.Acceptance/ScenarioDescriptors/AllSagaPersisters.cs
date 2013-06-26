namespace NServiceBus.Testing.Acceptance.ScenarioDescriptors
{
    using Support;

    public class AllSagaPersisters : ScenarioDescriptor
    {
        public AllSagaPersisters()
        {
            Add(SagaPersisters.InMemory);
            Add(SagaPersisters.Raven);
            Add(SagaPersisters.NHibernate);
        }
    }
}