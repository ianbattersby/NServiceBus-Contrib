namespace NServiceBus.Testing.Acceptance.Support
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RunDescriptorsBuilder
    {
        IList<RunDescriptor> descriptors = new List<RunDescriptor>();

        readonly List<string> excludes = new List<string>();
        public RunDescriptorsBuilder For<T>(params RunDescriptor[] runDescriptorsToExclude) where T : ScenarioDescriptor
        {
            this.excludes.AddRange(runDescriptorsToExclude
                .Where(r=>r != null)
                .Select(r =>r.Key.ToLowerInvariant()).ToArray());

            var sd = Activator.CreateInstance<T>() as ScenarioDescriptor;

            return this.For(sd.ToArray());
        }

        public RunDescriptorsBuilder For(params RunDescriptor[] descriptorsToAdd)
        {
            var toAdd = descriptorsToAdd.Where(r => r != null).ToList();

            if (!toAdd.Any())
            {
                this.emptyPermutationFound = true;
            }

            if (!this.descriptors.Any())
            {
                this.descriptors = toAdd;
                return this;
            }


            var result = new List<RunDescriptor>();

            foreach (var existingDescriptor in this.descriptors)
            {
                foreach (var descriptorToAdd in toAdd)
                {
                    var nd = new RunDescriptor(existingDescriptor);
                    nd.Merge(descriptorToAdd);
                    result.Add(nd);
                }
            }


            this.descriptors = result;

            return this;
        }

        public IList<RunDescriptor> Build()
        {
            //if we have found a empty permutation this means that we shouldn't run any permutations. This happens when a test is specified to run for a given key
            // but that key is not avaiable. Eg running tests for sql server but the sql transport isn't available
            if (this.emptyPermutationFound)
            {
                return new List<RunDescriptor>();
            }

            var environmentExcludes = GetEnvironmentExcludes();

            var activeDescriptors = this.descriptors.Where(d =>
                !this.excludes.Any(e => d.Key.ToLower().Contains(e)) &&
                !environmentExcludes.Any(e => d.Key.ToLower().Contains(e))
                ).ToList();

            int permutation = 1;
            foreach (var run in activeDescriptors)
            {
                run.Permutation = permutation;

                permutation++;

            }

            return activeDescriptors;
        }

        static IList<string> GetEnvironmentExcludes()
        {
            var env = Environment.GetEnvironmentVariable("nservicebus_test_exclude_scenarios");
            if (string.IsNullOrEmpty(env))
                return new List<string>();

            Console.Out.WriteLine("Scenarios excluded for this environment: " + env);
            return env.ToLower().Split(';');
        }

        bool emptyPermutationFound;

        
    }
}