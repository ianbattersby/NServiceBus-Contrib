namespace NServiceBus.Testing.Acceptance.Tests.Handlers
{
    using NServiceBus.Testing.Acceptance.Tests.Commands;

    /// <summary>
    /// Purposely not injecting test 'Context' here as this represents a production code
    /// test, and you wouldn't want to pollute your production code with test contexts.
    /// </summary>
    public class OrderPizzaHandler : IHandleMessages<OrderPizzaCommand>
    {
        public void Handle(OrderPizzaCommand message)
        {
        }
    }
}