namespace NServiceBus.Testing.Acceptance.Support
{
    using NServiceBus;
    using NServiceBus.Config.ConfigurationSource;

    public interface IEndpointSetupTemplate
    {
        Configure GetConfiguration(RunDescriptor runDescriptor, EndpointConfiguration endpointConfiguration, IConfigurationSource configSource);
    }
}