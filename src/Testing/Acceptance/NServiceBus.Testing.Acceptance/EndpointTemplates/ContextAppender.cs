namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using log4net.Appender;
    using log4net.Core;

    using NServiceBus.Testing.Acceptance;
    using NServiceBus.Testing.Acceptance.Support;

    public class ContextAppender : AppenderSkeleton
    {
        public ContextAppender(ScenarioContext context, EndpointConfiguration endpointConfiguration)
        {
            this.context = context;
            this.endpointConfiguration = endpointConfiguration;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent.ExceptionObject != null)
            {
                lock (this.context)
                {
                    this.context.AddException(loggingEvent.ExceptionObject);
                }
            }
        }

        ScenarioContext context;

        readonly EndpointConfiguration endpointConfiguration;
    }
}