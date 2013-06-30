﻿namespace NServiceBus.Testing.Acceptance.EndpointTemplates
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
            if (typeof(DefaultContext).IsAssignableFrom(this.Context.GetType()))
                ((DefaultContext)this.Context).UnitOfWorkStartedCount++;
        }

        public void End(Exception ex = null)
        {
            if (typeof(DefaultContext).IsAssignableFrom(this.Context.GetType()))
                ((DefaultContext)this.Context).UnitOfWorkEndedCount++;
        }
    }
}