﻿namespace NServiceBus.Testing.Acceptance.Support
{
    using System;
    using System.Collections.Generic;

    public interface IScenarioWithEndpointBehavior<TContext> where TContext : ScenarioContext
    {
        IScenarioWithEndpointBehavior<TContext> WithEndpoint<T>() where T : EndpointConfigurationBuilder;

        IScenarioWithEndpointBehavior<TContext> WithEndpoint<T>(Action<EndpointBehaviorBuilder<TContext>> behaviour) where T : EndpointConfigurationBuilder;

        IScenarioWithEndpointBehavior<TContext> Done(Func<TContext, bool> func);

        TContext Run(TimeSpan? testExecutionTimeout = null);

        IAdvancedScenarioWithEndpointBehavior<TContext> Repeat(Action<RunDescriptorsBuilder> runtimeDescriptor);

        IAdvancedScenarioWithEndpointBehavior<TContext> Should(Action<TContext> should);

    }

    public interface IAdvancedScenarioWithEndpointBehavior<TContext> where TContext : ScenarioContext
    {
        IAdvancedScenarioWithEndpointBehavior<TContext> Should(Action<TContext> should);

        IAdvancedScenarioWithEndpointBehavior<TContext> Report(Action<RunSummary> summaries);


        IAdvancedScenarioWithEndpointBehavior<TContext> MaxTestParallelism(int maxParallelism);

        IEnumerable<TContext> Run(TimeSpan? testExecutionTimeout = null);
    }
}