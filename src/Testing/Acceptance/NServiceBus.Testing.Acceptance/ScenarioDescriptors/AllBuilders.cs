namespace NServiceBus.Testing.Acceptance.ScenarioDescriptors
{
    using Support;

    public class AllBuilders:ScenarioDescriptor
    {
        public AllBuilders()
        {
            Add(Builders.StructureMap);
        }
    }
}