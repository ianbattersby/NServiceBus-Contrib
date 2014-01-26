namespace NServiceBus.Testing.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Text;

    [Intercept]
    [Serializable]
    public abstract class ScenarioContext : ContextBoundObject
    {
        public event EventHandler ContextPropertyChanged;

        private readonly ScenarioExceptionList exceptions;

        protected ScenarioContext()
        {
            this.exceptions = new ScenarioExceptionList();
        }

        [AttributeUsage(AttributeTargets.Class)]
        sealed class InterceptAttribute : ContextAttribute, IContributeObjectSink
        {
            public InterceptAttribute()
                : base("InterceptProperty")
            {
            }

            public override void GetPropertiesForNewContext(IConstructionCallMessage message)
            {
                message.ContextProperties.Add(this);
            }

            public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink)
            {
                return new InterceptSink { Target = (ScenarioContext)obj, NextSink = nextSink };
            }
        }

        class InterceptSink : IMessageSink
        {
            public IMessageSink NextSink { get; set; }

            public ScenarioContext Target;

            public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink sink)
            {
                throw new NotSupportedException("AsyncProcessMessage is not supported.");
            }

            public IMessage SyncProcessMessage(IMessage msg)
            {
                var call = msg as IMethodCallMessage;
                if (call != null)
                {
                    var method = call.MethodName;


                    if (this.Target.ContextPropertyChanged != null && method.StartsWith("set"))
                    {
                        this.Target.ContextPropertyChanged(this.Target, EventArgs.Empty);
                    }
                }

                return this.NextSink.SyncProcessMessage(msg);
            }
        }

        public bool EndpointsStarted { get; set; }

        public int UnitOfWorkStartedCount { get; set; }

        public int UnitOfWorkEndedCount { get; set; }

        public int SubscriptionsCount { get; set; }

        public IEnumerable<Exception> Exceptions
        {
            get
            {
                return this.exceptions;
            }
        }

        public string ExceptionLog
        {
            get
            {
                return this.exceptions.ToString();
            }
        }

        public void AddException(Exception ex)
        {
            this.exceptions.AddException(ex);
        }
    }

    [Serializable]
    public class ScenarioExceptionList : IEnumerable<Exception>
    {
        private readonly IList<Exception> exceptions;

        public ScenarioExceptionList()
        {
            this.exceptions = new List<Exception>();
        }

        public void AddException(Exception ex)
        {
            this.exceptions.Add(ex);
        }

        public IEnumerator<Exception> GetEnumerator()
        {
            return this.exceptions.GetEnumerator();
        }

        global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            return StringifyExceptions(this);
        }

        private static string StringifyExceptions(IEnumerable<Exception> exceptions)
        {
            var sb = new StringBuilder();

            foreach (var ex in exceptions)
            {
                sb.AppendLine((ex is AggregateException) ? StringifyExceptions((ex as AggregateException).InnerExceptions) : ex == null ? "(null)" : ex.ToString());
            }

            return sb.ToString();
        }
    }
}