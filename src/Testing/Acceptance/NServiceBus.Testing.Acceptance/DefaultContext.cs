namespace NServiceBus.Testing.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class DefaultContext : ScenarioContext
    {
        private readonly ExceptionList exceptions;

        public DefaultContext()
        {
            this.exceptions = new ExceptionList();
        }

        public bool UnitOfWorkStarted { get; set; }

        public bool UnitOfWorkEnded { get; set; }

        public int UnitOfWorkCount { get; set; }

        public int CallbackCount { get; set; }

        public IEnumerable<Exception> Exceptions
        {
            get
            {
                return this.exceptions;
            }
        }

        public void AddException(Exception ex)
        {
            this.exceptions.AddException(ex);
        }

    }

    [Serializable]
    public class ExceptionList : IEnumerable<Exception>
    {
        private readonly IList<Exception> exceptions;

        public ExceptionList()
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
                sb.AppendLine((ex is AggregateException) ? StringifyExceptions((ex as AggregateException).InnerExceptions) : ex.ToString());
            }

            return sb.ToString();
        }
    }
}