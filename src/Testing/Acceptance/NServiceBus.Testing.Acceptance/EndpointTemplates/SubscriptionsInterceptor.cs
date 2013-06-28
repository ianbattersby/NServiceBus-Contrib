namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using System;

    using NServiceBus.Transports;

    public class SubscriptionsInterceptor : IManageSubscriptions
    {
        public ScenarioContext Context { get; set; }

        public void Init()
        {
            Configure.Instance.Configurer.ConfigureComponent<SubscriptionsInterceptor>(DependencyLifecycle.SingleInstance);
        }

        public void Subscribe(Type eventType, Address publisherAddress)
        {
            if (typeof(DefaultContext).IsAssignableFrom(this.Context.GetType()))
            {
                ((DefaultContext)this.Context).SubscriptionsCount++;
            }
        }

        public void Unsubscribe(Type eventType, Address publisherAddress)
        {
            if (typeof(DefaultContext).IsAssignableFrom(this.Context.GetType()))
            {
                ((DefaultContext)this.Context).UnsubscriptionsCount++;
            }
        }
    }
}