namespace NServiceBus.Testing.Acceptance.Support
{
    public class DefaultScenarioDescriptor : ScenarioDescriptor
    {
        public DefaultScenarioDescriptor()
        {
            this.Add(new RunDescriptor { Key = "Default Scenario" });
        }
    }
}