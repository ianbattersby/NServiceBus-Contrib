namespace NServiceBus.Testing.Acceptance.Support
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Remoting.Lifetime;
    using System.Threading;
    using System.Threading.Tasks;

    using NServiceBus;
    using NServiceBus.Installation.Environments;
    using NServiceBus.Logging;


    [Serializable]
    public class EndpointRunner : MarshalByRefObject
    {
        IStartableBus bus;
        Configure config;

        EndpointConfiguration configuration;
        ScenarioContext scenarioContext;
        EndpointBehaviour behaviour;
        Semaphore contextChanged = new Semaphore(0, int.MaxValue);
        bool stopped = false;

        public Result Initialize(RunDescriptor run, EndpointBehaviour endpointBehaviour, IDictionary<Type, string> routingTable, string endpointName)
        {
            try
            {
                this.behaviour = endpointBehaviour;
                this.scenarioContext = run.ScenarioContext;
                this.configuration = ((IEndpointConfigurationFactory)Activator.CreateInstance(endpointBehaviour.EndpointBuilderType)).Get();
                this.configuration.EndpointName = endpointName;

                if (!string.IsNullOrEmpty(this.configuration.CustomMachineName))
                {
                    NServiceBus.Support.RuntimeEnvironment.MachineNameAction = () => this.configuration.CustomMachineName;
                }

                //apply custom config settings
                endpointBehaviour.CustomConfig.ForEach(customAction => customAction(this.config));
                this.config = this.configuration.GetConfiguration(run, routingTable);



                if (this.scenarioContext != null)
                {
                    this.config.Configurer.RegisterSingleton(this.scenarioContext.GetType(), this.scenarioContext);
                    this.scenarioContext.ContextPropertyChanged += this.scenarioContext_ContextPropertyChanged;
                }


                this.bus = this.config.CreateBus();

                Configure.Instance.ForInstallationOn<Windows>().Install();

                Task.Factory.StartNew(() =>
                    {
                        while (!this.stopped)
                        {
                            this.contextChanged.WaitOne(TimeSpan.FromSeconds(5)); //we spin around each 5 s since the callback mechanism seems to be shaky

                            lock (this.behaviour)
                            {

                                foreach (var when in this.behaviour.Whens)
                                {
                                    if (this.executedWhens.Contains(when.Id))
                                        continue;

                                    if (when.ExecuteAction(this.scenarioContext, this.bus))
                                        this.executedWhens.Add(when.Id);
                                }
                            }
                        }
                    });

                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initalize endpoint " + endpointName, ex);
                return Result.Failure(ex);
            }
        }

        readonly IList<Guid> executedWhens = new List<Guid>();

        void scenarioContext_ContextPropertyChanged(object sender, EventArgs e)
        {
            this.contextChanged.Release();
        }


        public Result Start()
        {
            try
            {
                foreach (var given in this.behaviour.Givens)
                {
                    var action = given.GetAction(this.scenarioContext);

                    action(this.bus);
                }

                this.bus.Start();


                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to start endpoint " + this.configuration.EndpointName, ex);

                return Result.Failure(ex);
            }
        }

        public Result Stop()
        {
            try
            {
                this.stopped = true;

                this.scenarioContext.ContextPropertyChanged -= this.scenarioContext_ContextPropertyChanged;

                this.bus.Dispose();



                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to stop endpoint " + this.configuration.EndpointName, ex);

                return Result.Failure(ex);
            }
        }

        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            if (lease.CurrentState == LeaseState.Initial)
            {
                lease.InitialLeaseTime = TimeSpan.FromMinutes(2);
                lease.SponsorshipTimeout = TimeSpan.FromMinutes(2);
                lease.RenewOnCallTime = TimeSpan.FromSeconds(2);
            }
            return lease;
        }

        public string Name()
        {
            return AppDomain.CurrentDomain.FriendlyName;
        }

        static readonly ILog Logger = LogManager.GetLogger(typeof(EndpointRunner));

        [Serializable]
        public class Result : MarshalByRefObject
        {
            public string ExceptionMessage { get; set; }

            public bool Failed
            {
                get { return this.ExceptionMessage != null; }

            }

            public static Result Success()
            {
                return new Result();
            }

            public static Result Failure(Exception ex)
            {
                return new Result
                    {
                        ExceptionMessage = ex.ToString(),
                        ExceptionType = ex.GetType()
                    };
            }

            public Type ExceptionType { get; set; }
        }
    }
}