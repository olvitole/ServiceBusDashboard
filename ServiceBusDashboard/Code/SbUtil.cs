using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ServiceBusDashboard.Model;

namespace ServiceBusDashboard.Code
{
    public static class SbUtil
    {
        private static readonly Regex[] IgnoreQueuesRegex = Config.IgnoreQueues.ExtractRegexPatterns();
        private static readonly Regex[] IgnoreTopicRegex = Config.IgnoreTopics.ExtractRegexPatterns();
        private static readonly Regex[] IgnoreSubscriptionsRegex = Config.IgnoreSubscriptions.ExtractRegexPatterns();
        private static readonly SbExcludingSubscription[] SubscriptionsToBeExcluded =
            Config.ExcludeSubscriptions.Split(',').Select(x => x.Split('/')).Where(x => x.Length > 1)
                .Select(x => new SbExcludingSubscription { Topic = x[0], Subscription = x[1] }).ToArray();

        private static Regex[] ExtractRegexPatterns(this string value) => value.Split(',').Where(_ => !string.IsNullOrWhiteSpace(_)).Select(x => new Regex(x, RegexOptions.IgnoreCase)).ToArray();

        public static bool IsQueueIgnored(string queue) => IgnoreQueuesRegex.Any(regex => regex.IsMatch(queue));
        public static bool IsTopicIgnored(string topic) => IgnoreTopicRegex.Any(regex => regex.IsMatch(topic));
        public static bool IsSubscriptionIgnored(string subscription) => IgnoreSubscriptionsRegex.Any(regex => regex.IsMatch(subscription));
        public static bool IsSubscriptionExcluded(string topic, string subscription) => SubscriptionsToBeExcluded.Any(x => x.Topic == topic && x.Subscription == subscription);

    }
}