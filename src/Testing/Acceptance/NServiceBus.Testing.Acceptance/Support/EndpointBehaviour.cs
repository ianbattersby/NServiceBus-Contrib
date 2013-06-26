namespace NServiceBus.Testing.Acceptance.Support
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using Customization;

    using NServiceBus;

    [Serializable]
    public class EndpointBehaviour : MarshalByRefObject
    {
        public EndpointBehaviour(Type builderType)
        {
            this.EndpointBuilderType = builderType;
            this.EndpointName = Conventions.EndpointNamingConvention(builderType);
            this.CustomConfig = new List<Action<Configure>>();
        }

        public string EndpointName { get; private set; }

        public Type EndpointBuilderType { get; private set; }



        public List<IGivenDefinition> Givens { get; set; }
        public List<IWhenDefinition> Whens { get; set; }

        public List<Action<Configure>> CustomConfig { get; set; }
    }


    [Serializable]
    public class GivenDefinition<TContext> : IGivenDefinition where TContext : ScenarioContext
    {
        public GivenDefinition(Action<IBus> action)
        {
            this.givenAction2 = action;
        }

        public GivenDefinition(Action<IBus, TContext> action)
        {
            this.givenAction = action;
        }

        public Action<IBus> GetAction(ScenarioContext context)
        {
            if (this.givenAction2 != null)
                return bus => this.givenAction2(bus);

            return bus => this.givenAction(bus, (TContext)context);
        }

        readonly Action<IBus, TContext> givenAction;
        readonly Action<IBus> givenAction2;

    }

    [Serializable]
    public class WhenDefinition<TContext> : IWhenDefinition where TContext : ScenarioContext
    {
        public WhenDefinition(Predicate<TContext> condition, Action<IBus> action)
        {
            this.id = Guid.NewGuid();
            this.condition = condition;
            this.busAction = action;
        }

        public WhenDefinition(Predicate<TContext> condition, Action<IBus, TContext> actionWithContext)
        {
            this.id = Guid.NewGuid();
            this.condition = condition;
            this.busAndContextAction = actionWithContext;
        }

        public Guid Id { get { return this.id; } }

        public bool ExecuteAction(ScenarioContext context, IBus bus)
        {
            var c = context as TContext;

            if (!this.condition(c))
            {
                return false;
            }


            if (this.busAction != null)
            {
                this.busAction(bus);
            }
            else
            {
                this.busAndContextAction(bus, c);
          
            }

            Debug.WriteLine("Condition {0} has fired - Thread: {1} AppDomain: {2}", this.id, Thread.CurrentThread.ManagedThreadId,AppDomain.CurrentDomain.FriendlyName);

            return true;
        }

        readonly Predicate<TContext> condition;
        readonly Action<IBus> busAction;
        readonly Action<IBus, TContext> busAndContextAction;
        Guid id;
    }

    public interface IGivenDefinition
    {
        Action<IBus> GetAction(ScenarioContext context);
    }


    public interface IWhenDefinition
    {
        bool ExecuteAction(ScenarioContext context, IBus bus);

        Guid Id { get; }
    }
}