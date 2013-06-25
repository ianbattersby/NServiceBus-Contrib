namespace NServiceBus.Testing.Acceptance.Support
{
    using System;
    using System.Collections.Generic;

    using NServiceBus;
    using NServiceBus.Features;
    using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

    public class EndpointBehaviorBuilder<TContext> where TContext:ScenarioContext
    {        
        public EndpointBehaviorBuilder(Type type)
        {
            this.behaviour = new EndpointBehaviour(type)
                {
                    Givens = new List<IGivenDefinition>(),
                    Whens = new List<IWhenDefinition>()
                };
        }

        public EndpointBehaviorBuilder<TContext> MonitorSubscriptions()
        {
            this.behaviour.Givens.Add(new GivenDefinition<TContext>((bus, context) =>
                {
                    if (!typeof(DefaultContext).IsAssignableFrom(context.GetType()))
                    {
                        throw new Exception("You must use DefaultContext, or inherit from it, to monitor subscriptions.");
                    }

                    if (Feature.IsEnabled<MessageDrivenSubscriptions>())
                    {
                        Configure.Instance.Builder.Build<MessageDrivenSubscriptionManager>().ClientSubscribed +=
                            (sender, args) => (context as DefaultContext).SubscriptionsCount++;
                    }
                }));

            return this;
        }

        public EndpointBehaviorBuilder<TContext> Given(Action<IBus> action)
        {
            this.behaviour.Givens.Add(new GivenDefinition<TContext>(action));

            return this;
        }

        public EndpointBehaviorBuilder<TContext> Given(Action<IBus,TContext> action)
        {
            this.behaviour.Givens.Add(new GivenDefinition<TContext>(action));

            return this;
        }

        public EndpointBehaviorBuilder<TContext> Subscribe<TEvent>() where TEvent : class
        {
            this.behaviour.Whens.Add(
                new WhenDefinition<TContext>(
                    context => context.EndpointsStarted,
                    (bus, context) =>
                        {
                            if (!typeof(DefaultContext).IsAssignableFrom(context.GetType()))
                            {
                                throw new Exception("You must use DefaultContext, or inherit from it, to monitor subscriptions.");
                            }

                            bus.Subscribe<TEvent>();

                            if (!Feature.IsEnabled<MessageDrivenSubscriptions>())
                                (context as DefaultContext).SubscriptionsCount++;
                        }));

            return this;
        }

        public EndpointBehaviorBuilder<TContext> When(Action<IBus> action)
        {
            return this.When(c => true, action);
        }

        public EndpointBehaviorBuilder<TContext> When(Predicate<TContext> condition, Action<IBus> action)
        {
            this.behaviour.Whens.Add(new WhenDefinition<TContext>(condition,action));

            return this;
        }

        public EndpointBehaviorBuilder<TContext> When(Predicate<TContext> condition, Action<IBus,TContext> action)
        {
            this.behaviour.Whens.Add(new WhenDefinition<TContext>(condition,action));

            return this;
        }

        public EndpointBehaviorBuilder<TContext> CustomConfig(Action<Configure> action)
        {
            this.behaviour.CustomConfig.Add(action);

            return this;
        }

        public EndpointBehaviour Build()
        {
            return this.behaviour;
        }

        readonly EndpointBehaviour behaviour;
    }
}