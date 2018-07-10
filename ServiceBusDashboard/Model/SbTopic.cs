using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceBusDashboard.Model
{
    public class SbTopic : CountContainer
    {
        public SbSubscription[] Subscriptions { get; set; }
        public long SubscriptionsActiveMessageCount => Subscriptions.Where(x => !x.Excluded).Select(x => x.ActiveMessageCount).Sum();
        public long SubscriptionsDeadLetterMessageCount => Subscriptions.Where(x => !x.Excluded).Select(x => x.DeadLetterMessageCount).Sum();

        public string SubscriptionsActiveMessageCountColor => SubscriptionsActiveMessageCount > 0 ? WarningClassName : DefaultClassName;
        public string SubscriptionsDeadLetterMessageCountColor => SubscriptionsDeadLetterMessageCount > 0 ? DangerClassName : DefaultClassName;

        public override bool ShowNumbers => base.ShowNumbers || SubscriptionsActiveMessageCount > 0 || SubscriptionsDeadLetterMessageCount > 0;

        protected override int DefaultCompareTo(CountContainer cc)
        {
            if (!(cc is SbTopic bb))
                return 0;

            if (bb.SubscriptionsDeadLetterMessageCount == SubscriptionsDeadLetterMessageCount)
            {
                return bb.SubscriptionsActiveMessageCount == SubscriptionsActiveMessageCount ?
                    base.DefaultCompareTo(bb) :
                    bb.SubscriptionsActiveMessageCount.CompareTo(SubscriptionsActiveMessageCount);
            }

            return bb.SubscriptionsDeadLetterMessageCount.CompareTo(SubscriptionsDeadLetterMessageCount);
        }
    }
}