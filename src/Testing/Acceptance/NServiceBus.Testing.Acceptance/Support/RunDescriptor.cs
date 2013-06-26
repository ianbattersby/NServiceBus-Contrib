﻿namespace NServiceBus.Testing.Acceptance.Support
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class RunDescriptor : MarshalByRefObject
    {
        protected bool Equals(RunDescriptor other)
        {
            return string.Equals(this.Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RunDescriptor) obj);
        }

        public override int GetHashCode()
        {
            return (this.Key != null ? this.Key.GetHashCode() : 0);
        }

        public RunDescriptor()
        {
            this.Settings = new Dictionary<string, string>();
        }

        public RunDescriptor(RunDescriptor template)
        {
            InitializeWithDescriptor(template);
        }

        public RunDescriptor(params RunDescriptor[] templates)
        {
            for (var i = 0; i < templates.Length; i++)
            {
                if (i == 0)
                    this.InitializeWithDescriptor(templates[i]);
                else
                    this.Merge(templates[i]);
            }
        }

        public string Key { get; set; }

        public IDictionary<string, string> Settings { get; set; }

        public ScenarioContext ScenarioContext { get; set; }

        public TimeSpan TestExecutionTimeout { get; set; }

        public int Permutation { get; set; }

        public void Merge(RunDescriptor descriptorToAdd)
        {
            this.Key += "." + descriptorToAdd.Key;

            foreach (var setting in descriptorToAdd.Settings)
            {
                this.Settings[setting.Key] = setting.Value;
            }
        }

        private void InitializeWithDescriptor(RunDescriptor descriptor)
        {
            this.Settings = descriptor.Settings.ToDictionary(entry => entry.Key,
                                                      entry => entry.Value);
            this.Key = descriptor.Key;
        }
    }
}