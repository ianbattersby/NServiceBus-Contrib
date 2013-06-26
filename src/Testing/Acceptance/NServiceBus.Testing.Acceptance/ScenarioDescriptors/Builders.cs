namespace NServiceBus.Testing.Acceptance.ScenarioDescriptors
{
    using System.Collections.Generic;

    using NServiceBus.ObjectBuilder.StructureMap;
    
    using Support;

    public static class Builders
    {
        public static readonly RunDescriptor StructureMap = new RunDescriptor
            {
                Key = "StructureMap",
                Settings =
                    new Dictionary<string, string>
                        {
                            {
                                "Builder", typeof (StructureMapObjectBuilder).AssemblyQualifiedName
                            }
                        }
            };
    }
}