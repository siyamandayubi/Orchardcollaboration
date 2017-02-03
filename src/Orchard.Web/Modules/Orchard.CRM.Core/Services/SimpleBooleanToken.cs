namespace Orchard.CRM.Core.Services
{
    using Orchard.Caching;
    using System;

    public class SimpleBooleanToken : IVolatileToken
    {
        public SimpleBooleanToken(Func<bool> evaluate)
        {
            this.Evaluate = evaluate;
        }

        public Func<bool> Evaluate { get; private set; }
        public bool IsCurrent
        {
            get
            {
                return this.Evaluate();
            }
            set
            {

            }
        }
    }
}