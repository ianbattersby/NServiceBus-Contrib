namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using System;

    using NServiceBus.UnitOfWork;

    public class UnitOfWorkInterceptor : IManageUnitsOfWork
    {
        public ScenarioContext Context { get; set; }

        public void Init()
        {
        }

        public void Begin()
        {
            if (typeof(ScenarioContext).IsAssignableFrom(this.Context.GetType()))
                ((ScenarioContext)this.Context).UnitOfWorkStartedCount++;
        }

        public void End(Exception ex = null)
        {
            if (typeof(ScenarioContext).IsAssignableFrom(this.Context.GetType()))
                ((ScenarioContext)this.Context).UnitOfWorkEndedCount++;
        }
    }
}