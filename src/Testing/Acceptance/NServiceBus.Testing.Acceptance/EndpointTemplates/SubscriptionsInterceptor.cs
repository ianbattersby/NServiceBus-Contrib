namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using System;

    using NServiceBus.Transports;

    public class SubscriptionsInterceptor : IManageSubscriptions
    {
        public DefaultContext Context { get; set; }

        public void Init()
        {
            Configure.Instance.Configurer.ConfigureComponent<SubscriptionsInterceptor>(DependencyLifecycle.InstancePerCall);
        }

        public void Subscribe(Type eventType, Address publisherAddress)
        {
            this.Context.SubscriptionsCount++;
        }

        public void Unsubscribe(Type eventType, Address publisherAddress)
        {
            this.Context.UnsubscriptionsCount++;
        }
    }
}