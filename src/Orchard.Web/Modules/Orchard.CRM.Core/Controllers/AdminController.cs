using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Controllers
{
    [ValidateInput(false)]
    [Admin]
    public class AdminController : Controller
    {
        // GET: Setup
        public ActionResult Index()
        {
            return View();
        }

        // GET: Setup
        public ActionResult Setup()
        {
            return View();
        }
    }
}