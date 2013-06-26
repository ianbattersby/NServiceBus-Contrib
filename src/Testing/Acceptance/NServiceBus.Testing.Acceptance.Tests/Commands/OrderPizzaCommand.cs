namespace NServiceBus.Testing.Acceptance.Tests.Commands
{
    using System;

    public class OrderPizzaCommand
    {
        public Guid Id { get; set; }

        public string CustomerName { get; set; }

        public string PizzaName { get; set; }
    }
}