namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using System;

    using NServiceBus;
    using NServiceBus.Faults;

    public class FailureHandler : IManageMessageFailures
    {
        public DefaultContext Context { get; set; }

        public void SerializationFailedForMessage(TransportMessage message, Exception e)
        {
            if (typeof(DefaultContext).IsAssignableFrom(this.Context.GetType()))
                this.Context.AddException(e);
        }

        public void ProcessingAlwaysFailsForMessage(TransportMessage message, Exception e)
        {
            if (typeof(DefaultContext).IsAssignableFrom(this.Context.GetType()))
                this.Context.AddException(e);
        }

        public void Init(Address address)
        {
        }
    }
}