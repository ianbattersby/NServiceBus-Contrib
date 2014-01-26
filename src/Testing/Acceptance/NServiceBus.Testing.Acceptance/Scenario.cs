namespace NServiceBus.Testing.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Customization;
    using Support;

    public class Scenario
    {
        public static IScenarioWithEndpointBehavior<ScenarioContext> Define()
        {
            return Define<ScenarioContext>();
        }

        public static IScenarioWithEndpointBehavior<T> Define<T>() where T : ScenarioContext
        {
            return new ScenarioWithContext<T>(Activator.CreateInstance<T>);
        }

        public static IScenarioWithEndpointBehavior<T> Define<T>(T context) where T : ScenarioContext
        {
            return new ScenarioWithContext<T>(() => context);
        }

        public static IScenarioWithEndpointBehavior<T> Define<T>(Func<T> contextFactory) where T : ScenarioContext
        {
            return new ScenarioWithContext<T>(contextFactory);
        }

    }

    public class ScenarioWithContext<TContext> : IScenarioWithEndpointBehavior<TContext>, IAdvancedScenarioWithEndpointBehavior<TContext> where TContext : ScenarioContext
    {
        public ScenarioWithContext(Func<TContext> factory)
        {
            this.contextFactory = factory;
        }

        public IScenarioWithEndpointBehavior<TContext> WithEndpoint<T>() where T : EndpointConfigurationBuilder
        {
            return this.WithEndpoint<T>(b => { });
        }

        public IScenarioWithEndpointBehavior<TContext> WithEndpoint<T>(Action<EndpointBehaviorBuilder<TContext>> defineBehaviour) where T : EndpointConfigurationBuilder
        {

            var builder = new EndpointBehaviorBuilder<TContext>(typeof(T));

            defineBehaviour(builder);

            this.behaviours.Add(builder.Build());

            return this;
        }

        public IScenarioWithEndpointBehavior<TContext> Done(Func<TContext, bool> func)
        {
            if (typeof(ScenarioContext).IsAssignableFrom(typeof(TContext)))
                this.done = c =>
                    {
                        var predicate = func((TContext)c);

                        if (!predicate && (c as ScenarioContext).Exceptions.Any())
                        {
                            return true;
                        }

                        return predicate;
                    };
            else
                this.done = (c) => func((TContext)c);

            return this;
        }

        public IEnumerable<TContext> Run(TimeSpan? testExecutionTimeout = null)
        {
            var builder = new RunDescriptorsBuilder();

            this.runDescriptorsBuilderAction(builder);

            var runDescriptors = builder.Build();

            if (!runDescriptors.Any())
            {
                Console.Out.WriteLine("No active rundescriptors was found for this test, test will not be executed");
                return new List<TContext>();
            }

            foreach (var runDescriptor in runDescriptors)
            {
                runDescriptor.ScenarioContext = this.contextFactory();
                runDescriptor.TestExecutionTimeout = testExecutionTimeout ?? TimeSpan.FromSeconds(90);
            }

            var sw = new Stopwatch();

            sw.Start();
            ScenarioRunner.Run(runDescriptors, this.behaviours, this.shoulds, this.done, this.limitTestParallelismTo, this.reports);

            sw.Stop();

            Console.Out.WriteLine("Total time for testrun: {0}", sw.Elapsed);

            return runDescriptors.Select(r => (TContext)r.ScenarioContext);
        }

        public IAdvancedScenarioWithEndpointBehavior<TContext> Repeat(Action<RunDescriptorsBuilder> action)
        {
            this.runDescriptorsBuilderAction = action;

            return this;
        }

        public IAdvancedScenarioWithEndpointBehavior<TContext> MaxTestParallelism(int maxParallelism)
        {
            this.limitTestParallelismTo = maxParallelism;

            return this;
        }


        TContext IScenarioWithEndpointBehavior<TContext>.Run(TimeSpan? testExecutionTimeout)
        {
            return this.Run(testExecutionTimeout).Single();
        }

        public IAdvancedScenarioWithEndpointBehavior<TContext> Should(Action<TContext> should)
        {
            this.shoulds.Add(new ScenarioVerification<TContext>
            {
                ContextType = typeof(TContext),
                Should = should
            });

            return this;
        }


        public IAdvancedScenarioWithEndpointBehavior<TContext> Report(Action<RunSummary> reportActions)
        {
            this.reports = reportActions;
            return this;
        }


        int limitTestParallelismTo;
        readonly IList<EndpointBehaviour> behaviours = new List<EndpointBehaviour>();
        Action<RunDescriptorsBuilder> runDescriptorsBuilderAction = builder => builder.For(Conventions.DefaultRunDescriptor());
        IList<IScenarioVerification> shoulds = new List<IScenarioVerification>();
        public Func<ScenarioContext, bool> done = context => true;

        Func<TContext> contextFactory = () => Activator.CreateInstance<TContext>();
        Action<RunSummary> reports;
    }

    public class ScenarioVerification<T> : IScenarioVerification where T : ScenarioContext
    {
        public Action<T> Should { get; set; }
        public Type ContextType { get; set; }

        public void Verify(ScenarioContext context)
        {
            this.Should(((T)context));
        }
    }

    public interface IScenarioVerification
    {
        Type ContextType { get; set; }
        void Verify(ScenarioContext context);
    }
}