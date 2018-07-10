using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Microsoft.ServiceBus;
using ServiceBusDashboard.Code;
using WebGrease.Css.Extensions;

namespace ServiceBusDashboard.Controllers
{
    [RoutePrefix("dashboard")]
    public class DashboardController : ApiController
    {
        [HttpGet]
        [Route("{serviceBusName}")]
        public IHttpActionResult Get(string serviceBusName)
        {
            const string ignoreQueues = "";
            const string ignoreTopics = "";
            const string ignoreSubscriptions = "^test(.*)$,^example(.*)$";
            //const string replaceTheseStartWordsWithDots = "inputqueue.interxion.";
            const string replaceTheseStartWordsWithDots = "";
            const string excludeSubscriptions = "topicname/subscriptionname,topicname2/subscriptionname2";

            var sbConnectionString = SbConnectionStrings.Instance.ConnectionStrings.FirstOrDefault(x => x.Name == serviceBusName);
            var namespaceManager = NamespaceManager.CreateFromConnectionString(sbConnectionString.ConnectionString);

            var ignoreQueuesPatterns = ignoreQueues.Split(',').Where(_ => !string.IsNullOrWhiteSpace(_)).ToArray();
            var ignoreTopicPatterns = ignoreTopics.Split(',').Where(_ => !string.IsNullOrWhiteSpace(_)).ToArray();
            var ignoreSubscriptionsPatterns = ignoreSubscriptions.Split(',').Where(_ => !string.IsNullOrWhiteSpace(_)).ToArray();

            bool IsQueueIgnored(string queue) => ignoreQueuesPatterns.Any(pattern => Regex.IsMatch(queue, pattern, RegexOptions.IgnoreCase));
            bool IsTopicIgnored(string topic) => ignoreTopicPatterns.Any(pattern => Regex.IsMatch(topic, pattern, RegexOptions.IgnoreCase));
            bool IsSubscriptionIgnored(string subscription) => ignoreSubscriptionsPatterns.Any(pattern => Regex.IsMatch(subscription, pattern, RegexOptions.IgnoreCase));
            string ReplaceTheseStartWordsWithDots(string name)
            {
                if (string.IsNullOrWhiteSpace(replaceTheseStartWordsWithDots))
                    return name;

                replaceTheseStartWordsWithDots
                    .Split(',')
                    .ForEach(r => name = name.StartsWith(r) ? name.Replace(replaceTheseStartWordsWithDots, "...") : name);

                return name;
            }

            var queues = namespaceManager.GetQueues();
            var topics = namespaceManager.GetTopics();

            var data = new DashboardModel()
            {
                ConnectionString= sbConnectionString,
                Queues = queues
                    .Where(x => !IsQueueIgnored(x.Path))
                    .Select(x => new SbQueue()
                    {
                        Name = ReplaceTheseStartWordsWithDots(x.Path),
                        ActiveMessageCount = x.MessageCount,
                        DeadLetterMessageCount = x.MessageCountDetails.DeadLetterMessageCount
                    })
                    .ToArray(),
                Topics = topics
                    .Where(x => !IsTopicIgnored(x.Path))
                    .Select(x => new SbTopic()
                    {
                        Name = ReplaceTheseStartWordsWithDots(x.Path),
                        ActiveMessageCount = x.MessageCountDetails.ActiveMessageCount,
                        DeadLetterMessageCount = x.MessageCountDetails.DeadLetterMessageCount,
                        Subscriptions = namespaceManager.GetSubscriptions(x.Path)
                            .Where(xx => !IsSubscriptionIgnored(xx.Name))
                            .Select(xx => new SbSubscription()
                            {
                                Name = ReplaceTheseStartWordsWithDots(xx.Name),
                                ActiveMessageCount = xx.MessageCount,
                                DeadLetterMessageCount = xx.MessageCountDetails.DeadLetterMessageCount
                            })
                        .ToArray()
                    })
                    .ToArray()
            };

            var excludedSubscriptions = excludeSubscriptions.Split(',').Select(x => x.Split('/')).Select(x => new { Topic = x[0], Subscription = x[1] });
            data.Topics.ForEach(x => x.Subscriptions.ForEach(xx =>
              {
                  if (excludedSubscriptions.Any(xxx => xxx.Topic == x.Name && xxx.Subscription == xx.Name))
                      xx.Excluded = true;
              }));

            data.Sort();

            var body = RazorUtil.RenderViewToString("Dashboard", "~/Views/Home/DashboardView.cshtml", data);

            return Ok(body);
        }
    }

    public class DashboardModel
    {
        public SbConnectionString ConnectionString { get; set; }
        public SbQueue[] Queues { get; set; }
        public SbTopic[] Topics { get; set; }

        public void Sort()
        {
            Array.Sort(Queues);
            Array.Sort(Topics);
            Topics.ForEach(x => Array.Sort(x.Subscriptions));
        }
    }

    public class SbQueue : CountContainer
    {
    }

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

    public class SbSubscription : CountContainer
    {
        public bool Excluded { get; set; }
    }

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
