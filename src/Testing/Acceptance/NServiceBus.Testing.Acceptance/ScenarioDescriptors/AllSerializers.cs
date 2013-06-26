namespace NServiceBus.Testing.Acceptance.ScenarioDescriptors
{
    using Support;

    public class AllSerializers : ScenarioDescriptor
    {
        public AllSerializers()
        {
            Add(Serializers.Bson);
            Add(Serializers.Json);
            Add(Serializers.Xml);
            Add(Serializers.Binary);
        }
    }
}