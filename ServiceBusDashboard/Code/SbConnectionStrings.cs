using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace ServiceBusDashboard.Code
{
    public class SbConnectionStrings
    {
        private static SbConnectionStrings _instance;
        public SbConnectionString[] ConnectionStrings { get; private set; }

        public static SbConnectionStrings Instance => _instance ?? (_instance = new SbConnectionStrings());

        private SbConnectionStrings()
        {

        }

        public void Load()
        {
            var rootPath = HostingEnvironment.MapPath("~/");
            var connectionStrings = ConfigurationManager.AppSettings["SbConnectionStrings"];
            var connectionStringsFile = new FileInfo(Path.Combine(rootPath, connectionStrings));
            if (connectionStringsFile.Exists)
            {
                var json = File.ReadAllText(connectionStringsFile.FullName);
                ConnectionStrings = JsonConvert.DeserializeObject<SbConnectionString[]>(json);
            }
            else
            {
                ConnectionStrings = new[] { new SbConnectionString()
                {
                    Name = connectionStrings,
                    ConnectionString = connectionStrings
                } };
            }
        }
    }

    public class SbConnectionString
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
}