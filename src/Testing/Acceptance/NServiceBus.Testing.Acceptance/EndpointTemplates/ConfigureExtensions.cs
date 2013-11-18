namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;

    using NServiceBus.ObjectBuilder.Autofac;
    using NServiceBus.ObjectBuilder.CastleWindsor;
    using NServiceBus.ObjectBuilder.Common.Config;
    using NServiceBus.ObjectBuilder.Ninject;
    using NServiceBus.ObjectBuilder.Spring;
    using NServiceBus.ObjectBuilder.StructureMap;
    using NServiceBus.ObjectBuilder.Unity;
    using NServiceBus.Persistence.InMemory.SagaPersister;
    using NServiceBus.Persistence.InMemory.SubscriptionStorage;
    using NServiceBus.Persistence.Msmq.SubscriptionStorage;
    using NServiceBus.Persistence.NHibernate;
    using NServiceBus.Persistence.Raven.SagaPersister;
    using NServiceBus.Persistence.Raven.SubscriptionStorage;
    using NServiceBus.SagaPersisters.NHibernate;
    using NServiceBus.Serializers.Binary;
    using NServiceBus.Serializers.Json;
    using NServiceBus.Serializers.XML;
    using NServiceBus.Testing.Acceptance;
    using NServiceBus.Testing.Acceptance.Support;
    using NServiceBus.Unicast.Subscriptions.NHibernate;

    public static class ConfigureExtensions
    {
        public static string DefaultConnectionString = @"Server=localhost\sqlexpress;Database=nservicebus;Trusted_Connection=True;";

        public static string GetOrNull(this IDictionary<string, string> dictionary, string key)
        {
            if (!dictionary.ContainsKey(key))
            {
                return null;
            }

            return dictionary[key];
        }

        public static Configure DefineHowManySubscriptionMessagesToWaitFor(this Configure config, int numberOfSubscriptionsToWaitFor)
        {
            config.Configurer.ConfigureProperty<EndpointConfigurationBuilder.SubscriptionsSpy>(
                    spy => spy.NumberOfSubscriptionsToWaitFor, numberOfSubscriptionsToWaitFor);

            return config;
        }

        public static Configure DefineTransport(this Configure config, IDictionary<string, string> settings)
        {
            if (!settings.ContainsKey("Transport"))
                settings = ScenarioDescriptors.Transports.Default.Settings;

            var transportType = Type.GetType(settings["Transport"]);

            return config.UseTransport(transportType, () => settings["Transport.ConnectionString"]);

        }

        public static Configure DefineSerializer(this Configure config, string serializer)
        {
            if (string.IsNullOrEmpty(serializer))
                return config;//xml is the default

            var type = Type.GetType(serializer);

            if (type == typeof (XmlMessageSerializer))
            {
                Configure.Serialization.Xml();
                return config;
            }


            if (type == typeof (JsonMessageSerializer))
            {
                Configure.Serialization.Json();
                return config;
            }

            if (type == typeof(BsonMessageSerializer))
            {
                Configure.Serialization.Bson();
                return config;
            }

            if (type == typeof (BinaryMessageSerializer))
            {
                Configure.Serialization.Binary();
                return config;
            }

            throw new InvalidOperationException("Unknown serializer:" + serializer);
        }


        public static Configure DefineSagaPersister(this Configure config, string persister, string connectionString)
        {
            if (string.IsNullOrEmpty(persister))
                return config.InMemorySagaPersister();

            var type = Type.GetType(persister);

            if (type == typeof(InMemorySagaPersister))
                return config.InMemorySagaPersister();

            if (type == typeof(RavenSagaPersister))
            {
                config.RavenPersistence(() => "url=http://localhost:8080");
                return config.RavenSagaPersister();

            }

            if (type == typeof(SagaPersister))
            {
                NHibernateSettingRetriever.ConnectionStrings = () =>
                {
                    var c = new ConnectionStringSettingsCollection();

                    c.Add(new ConnectionStringSettings("NServiceBus/Persistence", connectionString ?? DefaultConnectionString));
                    return c;

                };
                return config.UseNHibernateSagaPersister();
            }

            throw new InvalidOperationException("Unknown persister:" + persister);
        }


        public static Configure DefineSubscriptionStorage(this Configure config, string persister, string connectionString = null)
        {
            if (string.IsNullOrEmpty(persister))
                return config.InMemorySubscriptionStorage();

            var type = Type.GetType(persister);

            if (type == typeof(InMemorySubscriptionStorage))
                return config.InMemorySubscriptionStorage();

            if (type == typeof(RavenSubscriptionStorage))
            {
                config.RavenPersistence(() => "url=http://localhost:8080");
                return config.RavenSubscriptionStorage();

            }

            if (type == typeof(SubscriptionStorage))
            {
                NHibernateSettingRetriever.ConnectionStrings = () =>
                    {
                        var c = new ConnectionStringSettingsCollection();
                        
                        c.Add(new ConnectionStringSettings("NServiceBus/Persistence", connectionString ?? DefaultConnectionString));
                        return c;

                    };
                return config.UseNHibernateSubscriptionPersister();
            }


            if (type == typeof(MsmqSubscriptionStorage))
            {
                return config.MsmqSubscriptionStorage();
            }

            throw new InvalidOperationException("Unknown persister:" + persister);
        }

        public static Configure DefineBuilder(this Configure config, string builder)
        {
            if (string.IsNullOrEmpty(builder))
                return config.DefaultBuilder();

            var type = Type.GetType(builder);

            if (type == typeof(AutofacObjectBuilder))
            {
                ConfigureCommon.With(config, new AutofacObjectBuilder(null));

                return config;
            }

            if (type == typeof(WindsorObjectBuilder))
                return config.CastleWindsorBuilder();

            if (type == typeof(NinjectObjectBuilder))
                return config.NinjectBuilder();

            if (type == typeof(SpringObjectBuilder))
                return config.SpringFrameworkBuilder();

            if (type == typeof(StructureMapObjectBuilder))
                return config.StructureMapBuilder();

            if (type == typeof(UnityObjectBuilder))
                return config.StructureMapBuilder();


            throw new InvalidOperationException("Unknown builder:" + builder);
        }

        public static void SetupLogging(this EndpointConfiguration endpointConfiguration)
        {
            var logDir = ".\\logfiles\\";

            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            var logFile = Path.Combine(logDir, endpointConfiguration.EndpointName + "-" + endpointConfiguration.BuilderType.Name + ".txt");

            if (File.Exists(logFile))
                File.Delete(logFile);

            var logLevel = "DEBUG";
            var logLevelOverride = Environment.GetEnvironmentVariable("tests_loglevel");

            if (!string.IsNullOrEmpty(logLevelOverride))
                logLevel = logLevelOverride;

            SetLoggingLibrary.Log4Net(
                null, NServiceBus.Logging.Loggers.Log4NetAdapter.Log4NetAppenderFactory.CreateRollingFileAppender(logLevel, logFile));
        }
    }
}