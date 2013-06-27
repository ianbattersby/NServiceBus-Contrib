namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using System;

    using NServiceBus;
    using NServiceBus.UnitOfWork;

    public class UnitOfWorkInterceptor : IManageUnitsOfWork
    {
        public DefaultContext Context { get; set; }

        public void Init()
        {
            Configure.Instance.Configurer.ConfigureComponent<UnitOfWorkInterceptor>(DependencyLifecycle.InstancePerCall);
        }

        public void Begin()
        {
            this.Context.UnitOfWorkStartedCount++;
        }

        public void End(Exception ex = null)
        {
            this.Context.UnitOfWorkEndedCount++;
        }
    }
}