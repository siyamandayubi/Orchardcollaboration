namespace Orchard.CRM.Core.Controllers
{
    using Orchard.CRM.Core.Services;
    using System.Web.Mvc;

    public class CommandController : Controller
    {
        private readonly ICRMSetup crmsetup;

        public CommandController(ICRMSetup crmsetup)
        {
            this.crmsetup = crmsetup;
        }

        public ActionResult RunAll()
        {
            this.crmsetup.AddBasicData();
            return this.Redirect("/OrchardLocal/Admin/Contents/List");
        }
    }
}