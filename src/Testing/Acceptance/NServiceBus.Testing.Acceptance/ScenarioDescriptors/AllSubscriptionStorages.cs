namespace NServiceBus.Testing.Acceptance.ScenarioDescriptors
{
    using Support;

    public class AllSubscriptionStorages : ScenarioDescriptor
    {
        public AllSubscriptionStorages()
        {
            Add(SubscriptionStorages.InMemory);
            Add(SubscriptionStorages.Raven);
            Add(SubscriptionStorages.NHibernate);
            Add(SubscriptionStorages.Msmq);
        }
    }
}