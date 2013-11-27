namespace NServiceBus.Testing.Acceptance.EndpointTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Runtime.Remoting.Messaging;

    using NHibernate.Util;

    using NServiceBus.ObjectBuilder.Autofac;
    using NServiceBus.ObjectBuilder.CastleWindsor;
    using NServiceBus.ObjectBuilder.Common.Config;
    using NServiceBus.ObjectBuilder.Ninject;
    using NServiceBus.ObjectBuilder.Spring;
    using NServiceBus.ObjectBuilder.StructureMap;
    using NServiceBus.ObjectBuilder.Unity;
    using NServiceBus.Persistence.InMemory.SagaPersister;
    using NServiceBus.Persistence.InMemory.SubscriptionStorage;
    using NServiceBus.Persistence.InMemory.TimeoutPersister;
    using NServiceBus.Persistence.Msmq.SubscriptionStorage;
    using NServiceBus.Persistence.NHibernate;
    using NServiceBus.Persistence.Raven.SagaPersister;
    using NServiceBus.Persistence.Raven.SubscriptionStorage;
    using NServiceBus.Persistence.Raven.TimeoutPersister;
    using NServiceBus.SagaPersisters.NHibernate;
    using NServiceBus.Serializers.Binary;
    using NServiceBus.Serializers.Json;
    using NServiceBus.Serializers.XML;
    using NServiceBus.Testing.Acceptance;
    using NServiceBus.Testing.Acceptance.Support;
    using NServiceBus.TimeoutPersisters.NHibernate;
    using NServiceBus.Unicast.Subscriptions.NHibernate;

    using StructureMap;
    using StructureMap.Configuration.DSL;

    public static class ConfigureExtensions
    {
        public static string DefaultConnectionString = @"Server=localhost\sqlexpress;Database=nservicebus;Trusted_Connection=True;";

        public static string DefaultNHibernateDialect = "NHibernate.Dialect.MsSql2008Dialect";

        public static string DefaultNHibernateDriver = "NHibernate.Driver.Sql2008ClientDriver";

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

            if (type == typeof(XmlMessageSerializer))
            {
                Configure.Serialization.Xml();
                return config;
            }


            if (type == typeof(JsonMessageSerializer))
            {
                Configure.Serialization.Json();
                return config;
            }

            if (type == typeof(BsonMessageSerializer))
            {
                Configure.Serialization.Bson();
                return config;
            }

            if (type == typeof(BinaryMessageSerializer))
            {
                Configure.Serialization.Binary();
                return config;
            }

            throw new InvalidOperationException("Unknown serializer:" + serializer);
        }


        public static Configure DefineSagaPersister(this Configure config, string persister, string connectionString, string dialect, string driver)
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
                AddAppSetting("NServiceBus/Persistence/NHibernate/dialect", dialect ?? DefaultNHibernateDialect);
                AddAppSetting("NServiceBus/Persistence/NHibernate/connection.driver_class", driver ?? DefaultNHibernateDriver);
                AddConnectionSetting("NServiceBus/Persistence/NHibernate/Saga", connectionString ?? DefaultConnectionString);

                return config.UseNHibernateSagaPersister();
            }

            throw new InvalidOperationException("Unknown saga persister:" + persister);
        }

        public static Configure DefineTimeoutPersister(this Configure config, string persister, string connectionString, string dialect, string driver)
        {
            if (string.IsNullOrEmpty(persister))
                return config.UseInMemoryTimeoutPersister();

            var type = Type.GetType(persister);

            if (type == typeof(InMemoryTimeoutPersistence))
                return config.UseInMemoryTimeoutPersister();

            if (type == typeof(RavenTimeoutPersistence))
            {
                config.RavenPersistence(() => "url=http://localhost:8080");
                return config.UseRavenTimeoutPersister();
            }

            if (type == typeof(TimeoutStorage))
            {
                AddAppSetting("NServiceBus/Persistence/NHibernate/dialect", dialect ?? DefaultNHibernateDialect);
                AddAppSetting("NServiceBus/Persistence/NHibernate/connection.driver_class", driver ?? DefaultNHibernateDriver);
                AddConnectionSetting("NServiceBus/Persistence/NHibernate/Timeout", connectionString ?? DefaultConnectionString);

                return config.UseNHibernateTimeoutPersister();
            }

            throw new InvalidOperationException("Unknown timeout persister:" + persister);
        }

        public static Configure DefineSubscriptionStorage(this Configure config, string persister, string connectionString, string dialect, string driver)
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
                AddAppSetting("NServiceBus/Persistence/NHibernate/dialect", dialect ?? DefaultNHibernateDialect);
                AddAppSetting("NServiceBus/Persistence/NHibernate/connection.driver_class", driver ?? DefaultNHibernateDriver);
                AddConnectionSetting("NServiceBus/Persistence/NHibernate/Subscription", connectionString ?? DefaultConnectionString);

                return config.UseNHibernateSubscriptionPersister();
            }


            if (type == typeof(MsmqSubscriptionStorage))
            {
                return config.MsmqSubscriptionStorage();
            }

            throw new InvalidOperationException("Unknown subscription persister:" + persister);
        }

        public static Configure DefineBuilder(this Configure config, string builder, string builderRegistry)
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
            {
                if (!String.IsNullOrWhiteSpace(builderRegistry))
                {
                    var registryType = Type.GetType(builderRegistry);
                    return config.StructureMapBuilder(new Container(
                        x =>
                        {
                            var registry = Activator.CreateInstance(registryType);
                            x.AddRegistry((Registry)registry);
                        }));
                }

                return config.StructureMapBuilder();
            }

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

        private static void AddAppSetting(string name, string value)
        {
            var settings = new NameValueCollection();

            if (NHibernateSettingRetriever.AppSettings != null)
            {
                settings = NHibernateSettingRetriever.AppSettings.Invoke();
            }

            if (settings[name] == null)
            {
                settings.Add(name, value);

                NHibernateSettingRetriever.AppSettings = () => settings;
            }
        }

        private static void AddConnectionSetting(string name, string connectionString)
        {
            var settings = new ConnectionStringSettingsCollection();

            if (NHibernateSettingRetriever.ConnectionStrings != null)
            {
                settings = NHibernateSettingRetriever.ConnectionStrings.Invoke();
            }

            if (settings[name] == null)
            {
                settings.Add(new ConnectionStringSettings(name, connectionString));

                NHibernateSettingRetriever.ConnectionStrings = () => settings;
            }
        }
    }
}