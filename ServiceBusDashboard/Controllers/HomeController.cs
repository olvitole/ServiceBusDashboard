using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceBusDashboard.Code;

namespace ServiceBusDashboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(SbConnectionStrings.Instance.ConnectionStrings);
        }

        public ActionResult ReloadList()
        {
            SbConnectionStrings.Instance.Load();
            return RedirectToAction("Index");
        }

        public ActionResult ServiceBus(string name)
        {
            return View(SbConnectionStrings.Instance.ConnectionStrings.FirstOrDefault(x => x.Name == name));
        }
    }
}
