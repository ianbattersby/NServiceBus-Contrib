namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Messaging;
    using System.Reflection;

    using NHibernate.Linq;

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

            SetLoggingLibrary.Log4Net(null, new ContextAppender(runDescriptor.ScenarioContext, endpointConfiguration));

            var types = GetTypesToUse(endpointConfiguration);

            var transportToUse = settings.GetOrNull("Transport");

            Configure.Features.Enable<Features.Sagas>();

            SettingsHolder.SetDefault("ScaleOut.UseSingleBrokerQueue", true);

            var config = Configure.With(types)
                            .DefineEndpointName(endpointConfiguration.EndpointName)
                            .DefineBuilder(settings.GetOrNull("Builder"), settings.GetOrNull("BuilderRegistryType"))
                            .CustomConfigurationSource(configSource)
                            .DefineSerializer(settings.GetOrNull("Serializer"))
                            .DefineTransport(settings)
                            .DefineSagaPersister(
                                settings.GetOrNull("SagaPersister"), 
                                settings.GetOrNull("SagaPersister.ConnectionString"),
                                settings.GetOrNull("NHibernate.Dialect"),
                                settings.GetOrNull("NHibernate.Driver"))
                            .DefineTimeoutPersister(
                                settings.GetOrNull("TimeoutPersister"),
                                settings.GetOrNull("TimeoutPersister.ConnectionString"),
                                settings.GetOrNull("NHibernate.Dialect"),
                                settings.GetOrNull("NHibernate.Driver"))
                            .DisableGateway();

            if (endpointConfiguration.CommandsDefinition != null)
                config.DefiningCommandsAs(endpointConfiguration.CommandsDefinition);

            if (endpointConfiguration.EventsDefinition != null)
                config.DefiningEventsAs(endpointConfiguration.EventsDefinition);

            if (endpointConfiguration.MessagesDefinition != null)
                config.DefiningMessagesAs(endpointConfiguration.MessagesDefinition);

            if (transportToUse == null || transportToUse.Contains("Msmq") || transportToUse.Contains("SqlServer"))
                config.DefineSubscriptionStorage(
                    settings.GetOrNull("SubscriptionStorage"),
                    settings.GetOrNull("SubscriptionPersister.ConnectionString"),
                                settings.GetOrNull("NHibernate.Dialect"),
                                settings.GetOrNull("NHibernate.Driver"));

            config.Configurer.ConfigureComponent<UnitOfWorkInterceptor>(DependencyLifecycle.SingleInstance);

            if (endpointConfiguration.PurgeOnStartup && transportToUse.Contains("Msmq"))
            {
                MessageQueue
                    .GetPrivateQueuesByMachine(".")
                    .Where(q => q.QueueName.StartsWith(string.Format("private$\\{0}", endpointConfiguration.EndpointName), StringComparison.InvariantCultureIgnoreCase))
                    .ToList()
                    .ForEach(q => q.Purge());
            }

            return config.UnicastBus();
        }

        static IEnumerable<Type> GetTypesToUse(EndpointConfiguration endpointConfiguration)
        {
            IEnumerable<Type> qualifyingTypes;

            if (endpointConfiguration.AssembliesToScan.Count > 0)
            {
                var assemblyTypes = new List<Type>();

                foreach (var assemblySpec in endpointConfiguration.AssembliesToScan)
                {
                    var assembly = assemblySpec.Item1;

                    if (assembly != Assembly.GetExecutingAssembly())
                    {
                        assemblyTypes.AddRange(
                            string.IsNullOrWhiteSpace(assemblySpec.Item2)
                            ? assembly.GetTypes()
                            : assembly.GetTypes().Where(t => t.Namespace.StartsWith(assemblySpec.Item2, StringComparison.InvariantCultureIgnoreCase)));
                    }
                }

                qualifyingTypes = assemblyTypes;
            }
            else
            {
                qualifyingTypes = new AssemblyScanner().GetScannableAssemblies().Assemblies
                            .Where(a => a != Assembly.GetExecutingAssembly())
                            .SelectMany(a => a.GetTypes());
            }

            var types = qualifyingTypes
                .Where(t => endpointConfiguration.NamespacesToExclude.Count == 0 || (t != null && !String.IsNullOrWhiteSpace(t.Namespace) && !endpointConfiguration.NamespacesToExclude.Any(e => t.Namespace.StartsWith(e, StringComparison.InvariantCultureIgnoreCase))))
                .Union(GetNestedTypeRecursive(endpointConfiguration.BuilderType.DeclaringType))
                .Where(t => !endpointConfiguration.TypesToExclude.Contains(t)).ToList();

            var exceptions = (from mappedType in endpointConfiguration.EndpointMappings.Select(x => x.Key) where !types.Contains(mappedType) select "Unable to find mapped type '" + mappedType.Name + "' in scanned assemblies.").ToList();

            if (exceptions.Any())
            {
                if (exceptions.Count > 1)
                    throw new AggregateException(exceptions.Select(x => new Exception(x)));
                else
                    throw new Exception(exceptions.ElementAt(0));
            }

            var messageCheck = endpointConfiguration.MessagesDefinition == null || types.Any(t => endpointConfiguration.MessagesDefinition(t));
            var commandCheck = endpointConfiguration.CommandsDefinition == null || types.Any(t => endpointConfiguration.CommandsDefinition(t));
            var eventCheck = endpointConfiguration.EventsDefinition == null || types.Any(t => endpointConfiguration.EventsDefinition(t));

            if (!(messageCheck && commandCheck && eventCheck))
            {
                throw new Exception(
                    string.Format("Unobtrusive-mode predicates exist for '{0}' with no matching types:\n  {1}", endpointConfiguration.BuilderType.FullName, string.Format("[VALID] -- MessagesAs: {0}, CommandsAs: {1}, EventsAs: {2}", messageCheck, commandCheck, eventCheck)));
            }

            return types;
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