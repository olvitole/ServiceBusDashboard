using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceBusDashboard.Model
{
    public class SbExcludingSubscription
    {
        public string Topic { get; set; }
        public string Subscription { get; set; }
    }
}