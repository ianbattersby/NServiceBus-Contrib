﻿namespace NServiceBus.Testing.Acceptance
{
    using System;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;

    [Intercept]
    [Serializable]
    public abstract class ScenarioContext : ContextBoundObject
    {
        public event EventHandler ContextPropertyChanged;

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
    }
}