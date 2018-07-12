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
using ServiceBusDashboard.Model;

namespace ServiceBusDashboard.Controllers
{
    [RoutePrefix("dashboard")]
    public class DashboardController : ApiController
    {
        [HttpGet]
        [Route("{serviceBusName}")]
        public IHttpActionResult Get(string serviceBusName)
        {
            var sbConnectionString = SbConnectionStrings.Instance.ConnectionStrings.FirstOrDefault(x => x.Name == serviceBusName);
            if (sbConnectionString == null)
                return NotFound();

            var namespaceManager = NamespaceManager.CreateFromConnectionString(sbConnectionString.ConnectionString);

            var queues = namespaceManager.GetQueues();
            var topics = namespaceManager.GetTopics();

            var data = new SbDashboardModel()
            {
                ConnectionString = sbConnectionString,
                Queues = queues
                    .Where(x => !SbUtil.IsQueueIgnored(x.Path))
                    .Select(x => new SbQueue()
                    {
                        Name = x.Path,
                        ActiveMessageCount = x.MessageCountDetails.ActiveMessageCount,
                        DeadLetterMessageCount = x.MessageCountDetails.DeadLetterMessageCount
                    })
                    .ToArray(),
                Topics = topics
                    .Where(x => !SbUtil.IsTopicIgnored(x.Path))
                    .Select(x => new SbTopic()
                    {
                        Name = x.Path,
                        ActiveMessageCount = x.MessageCountDetails.ActiveMessageCount,
                        DeadLetterMessageCount = x.MessageCountDetails.DeadLetterMessageCount,
                        Subscriptions = namespaceManager.GetSubscriptions(x.Path)
                            .Where(xx => !SbUtil.IsSubscriptionIgnored(xx.Name))
                            .Select(xx => new SbSubscription()
                            {
                                Name = xx.Name,
                                ActiveMessageCount = xx.MessageCountDetails.ActiveMessageCount,
                                DeadLetterMessageCount = xx.MessageCountDetails.DeadLetterMessageCount
                            })
                        .ToArray()
                    })
                    .ToArray()
            };

            // Check if any subscription is excluded
            foreach (var topic in data.Topics)
            {
                foreach (var subscription in topic.Subscriptions)
                {
                    if (SbUtil.IsSubscriptionExcluded(topic.Name, subscription.Name))
                        subscription.Excluded = true;
                }
            }

            // Sorting all the entities
            data.Sort();

            var body = RazorUtil.RenderViewToString("Dashboard", "~/Views/Home/DashboardView.cshtml", data);

            return Ok(body);
        }
    }
}
