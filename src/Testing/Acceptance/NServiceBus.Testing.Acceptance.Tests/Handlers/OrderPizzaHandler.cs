namespace NServiceBus.Testing.Acceptance.Tests.Handlers
{
    using System;

    using NServiceBus.Testing.Acceptance.Tests.Commands;

    /// <summary>
    /// Purposely not injecting test 'Context' here as this represents a production code
    /// test, and you wouldn't want to pollute your production code with test contexts.
    /// </summary>
    public class OrderPizzaHandler : IHandleMessages<OrderPizzaCommand>
    {
        public ComplexContext Context { get; set; }

        public void Handle(OrderPizzaCommand message)
        {
            this.Context.SomeString = "Ian Was Ere";
            this.Context.SomeGuid = new Guid("AE60893B-4B57-44D1-BDE7-6AA805D0C7AF");
        }
    }
}