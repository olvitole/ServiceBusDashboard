using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceBusDashboard.Model
{
    public abstract class CountContainer : IComparable
    {
        protected const string DefaultClassName = "badge badge-info center-block";
        protected const string WarningClassName = "badge badge-warning center-block";
        protected const string DangerClassName = "badge badge-danger center-block";

        public string Name { get; set; }
        public long ActiveMessageCount { get; set; }
        public long DeadLetterMessageCount { get; set; }

        public string ActiveMessageCountColor => ActiveMessageCount > 0 ? WarningClassName : DefaultClassName;
        public string DeadLetterMessageCountColor => DeadLetterMessageCount > 0 ? DangerClassName : DefaultClassName;

        public virtual bool ShowNumbers => ActiveMessageCount > 0 || DeadLetterMessageCount > 0;

        public virtual int CompareTo(object b)
        {
            if (!(b is CountContainer bb))
                return 0;

            if (bb.DeadLetterMessageCount == DeadLetterMessageCount)
            {
                return bb.ActiveMessageCount == ActiveMessageCount ?
                    DefaultCompareTo(bb) :
                    bb.ActiveMessageCount.CompareTo(ActiveMessageCount);
            }

            return bb.DeadLetterMessageCount.CompareTo(DeadLetterMessageCount);
        }

        protected virtual int DefaultCompareTo(CountContainer cc)
        {
            return string.Compare(Name, cc.Name, StringComparison.Ordinal);
        }
    }
}