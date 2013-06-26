﻿namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using NServiceBus;
    using NServiceBus.Config.ConfigurationSource;
    using NServiceBus.Hosting.Helpers;
    using NServiceBus.Settings;
    using NServiceBus.Testing.Acceptance.Support;

    public class DefaultServer : IEndpointSetupTemplate
    {
        public Configure GetConfiguration(RunDescriptor runDescriptor, EndpointConfiguration endpointConfiguration, IConfigurationSource configSource)
        {
            var settings = runDescriptor.Settings;

            endpointConfiguration.SetupLogging();

            var types = GetTypesToUse(endpointConfiguration);

            var transportToUse = settings.GetOrNull("Transport");
            
            Configure.Features.Enable<Features.Sagas>();
            SettingsHolder.SetDefault("ScaleOut.UseSingleBrokerQueue", true);

            var config = Configure.With(types)
                            .DefineEndpointName(endpointConfiguration.EndpointName)
                            .DefineBuilder(settings.GetOrNull("Builder"))
                            .CustomConfigurationSource(configSource)
                            .DefineSerializer(settings.GetOrNull("Serializer"))
                            .DefineTransport(settings)
                            .DefineSagaPersister(settings.GetOrNull("SagaPersister"));

            if (transportToUse == null || 
                transportToUse.Contains("Msmq") || 
                transportToUse.Contains("SqlServer") || 
                transportToUse.Contains("RabbitMq") || 
                transportToUse.Contains("AzureServiceBus") ||
                transportToUse.Contains("AzureStorageQueue"))
                config.UseInMemoryTimeoutPersister();

            if (transportToUse == null || transportToUse.Contains("Msmq") || transportToUse.Contains("SqlServer"))
                config.DefineSubscriptionStorage(settings.GetOrNull("SubscriptionStorage"));

            config.Configurer.ConfigureComponent<FailureHandler>(DependencyLifecycle.SingleInstance);
            config.Configurer.ConfigureComponent<UnitOfWorkInterceptor>(DependencyLifecycle.InstancePerCall);

            return config.UnicastBus();
        }

        static IEnumerable<Type> GetTypesToUse(EndpointConfiguration endpointConfiguration)
        {
            var assemblies = AssemblyScanner.GetScannableAssemblies();

            var types = assemblies.Assemblies
                                    //exclude all test types by default
                                  .Where(a => a != Assembly.GetExecutingAssembly())
                                  .SelectMany(a => a.GetTypes());


            types = types.Union(GetNestedTypeRecursive(endpointConfiguration.BuilderType.DeclaringType));

            return types.Where(t=>!endpointConfiguration.TypesToExclude.Contains(t)).ToList();
        }

        static IEnumerable<Type> GetNestedTypeRecursive(Type rootType)
        {
            yield return rootType;

            foreach (var nestedType in rootType.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SelectMany(GetNestedTypeRecursive))
            {
                yield return nestedType;
            }
        }
    }
}