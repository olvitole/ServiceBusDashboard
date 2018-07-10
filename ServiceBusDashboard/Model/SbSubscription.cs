using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceBusDashboard.Model
{
    public class SbSubscription : CountContainer
    {
        public bool Excluded { get; set; }
    }
}