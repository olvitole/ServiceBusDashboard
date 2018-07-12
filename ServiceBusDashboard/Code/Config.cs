using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ServiceBusDashboard.Code
{
    public static class Config
    {
        public static string IgnoreQueues => ConfigurationManager.AppSettings["IgnoreQueues"];
        public static string IgnoreTopics => ConfigurationManager.AppSettings["IgnoreTopics"];
        public static string IgnoreSubscriptions => ConfigurationManager.AppSettings["IgnoreSubscriptions"];
        public static string ExcludeSubscriptions => ConfigurationManager.AppSettings["ExcludeSubscriptions"];
        public static int RefreshEvery => Convert.ToInt32(ConfigurationManager.AppSettings["RefreshEvery"]);
    }
}