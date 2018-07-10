using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceBusDashboard.Code;

namespace ServiceBusDashboard.Model
{
    public class SbDashboardModel
    {
        public SbConnectionString ConnectionString { get; set; }
        public SbQueue[] Queues { get; set; }
        public SbTopic[] Topics { get; set; }

        public void Sort()
        {
            Array.Sort(Queues);
            Array.Sort(Topics);

            foreach (var topic in Topics)
                Array.Sort(topic.Subscriptions);
        }
    }
}