using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Controllers
{
    [ValidateInput(false), Admin]
    public class EmailTemplateController : Controller
    {
        private IRepository<EmailTemplateRecord> emailTemplateRepository;
        private IOrchardServices services;

        public Localizer T { get; set; }

        public EmailTemplateController(IRepository<EmailTemplateRecord> emailTemplateRepository, IOrchardServices services)
        {
            this.services = services;
            this.emailTemplateRepository = emailTemplateRepository;
        }

        // GET: EmailTemplate
        public ActionResult Index()
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission, T("Not authorized")))
            {
                return new HttpUnauthorizedResult();
            }

            var emailTempalates = this.emailTemplateRepository.Table.ToList();

            var model = emailTempalates.Select(c => new EmailTemplateViewModel
            {
                EmailTemplateId = c.Id,
                Name = c.Name,
                TypeId = c.TypeId,
                Subject = c.Subject,
                Text = c.Body,
                TypeName = Enum.GetName(typeof(EmailTemplateType), c.TypeId)
            }).ToList();

            foreach (var template in model)
            {
                EmailTemplateType templateType = (EmailTemplateType)template.TypeId;
                switch (templateType)
                {
                    case EmailTemplateType.TicketCreated:
                        template.TypeName = EmailTemplateTypeNames.TicketCreatedName;
                        break;
                    case EmailTemplateType.TicketClosed:
                        template.TypeName = EmailTemplateTypeNames.TicketClosededName;
                        break;
                    case EmailTemplateType.NewMessage:
                        template.TypeName = EmailTemplateTypeNames.NewMessageName;
                        break;
                    case EmailTemplateType.TicketAssignedToUser:
                        template.TypeName = EmailTemplateTypeNames.TicketAssignedToUserName;
                        break;
                    default:
                        break;
                }
            }

            return this.View(model);
        }

        public ActionResult Create()
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var model = new EmailTemplateViewModel();
            this.FillByTypeItems(model.Types, null);

            return this.View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var emailTemplate = this.emailTemplateRepository.Table.FirstOrDefault(c => c.Id == id);

            var model = new EmailTemplateViewModel
            {
                EmailTemplateId = emailTemplate.Id,
                Name = emailTemplate.Name,
                TypeId = emailTemplate.TypeId,
                Text = emailTemplate.Body,
                Subject = emailTemplate.Subject
            };

            this.FillByTypeItems(model.Types, model.TypeId);

            return this.View(model);
        }

        [HttpPost]
        public ActionResult EditPost(EmailTemplateViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                model = model ?? new EmailTemplateViewModel();
                this.FillByTypeItems(model.Types, null);
                return this.View("Edit", model);
            }

            var emailTemplate = this.emailTemplateRepository.Table.FirstOrDefault(c => c.Id == model.EmailTemplateId);

            if (emailTemplate == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no emailTemplate with the given Id").ToString());
                return this.View(model);
            }

            emailTemplate.Name = model.Name;
            emailTemplate.TypeId = model.TypeId;
            emailTemplate.Body = model.Text;
            emailTemplate.Subject = model.Subject;
            this.emailTemplateRepository.Flush();

            return RedirectToAction("Index");
        }

        public ActionResult Remove(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var emailTemplate = this.emailTemplateRepository.Table.FirstOrDefault(c => c.Id == id);

            if (emailTemplate == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no emailTemplate with the given Id").ToString());
                return this.View("Index");
            }

            this.emailTemplateRepository.Delete(emailTemplate);
            this.emailTemplateRepository.Flush();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CreatePost(EmailTemplateViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                model = model ?? new EmailTemplateViewModel();
                this.FillByTypeItems(model.Types, null);
                return this.View("Create", model);
            }

            EmailTemplateRecord emailTemplate = new EmailTemplateRecord();

            emailTemplate.Name = model.Name;
            emailTemplate.TypeId = model.TypeId;
            emailTemplate.Body = model.Text;
            emailTemplate.Subject = model.Subject;

            this.emailTemplateRepository.Create(emailTemplate);
            this.emailTemplateRepository.Flush();

            return RedirectToAction("Index");
        }

        private void FillByTypeItems(Collection<SelectListItem> collection, int? selectedItem)
        {
            collection.Add(new SelectListItem { Text = EmailTemplateTypeNames.TicketCreatedName, Value = ((byte)EmailTemplateType.TicketCreated).ToString(CultureInfo.InvariantCulture) });
            collection.Add(new SelectListItem { Text = EmailTemplateTypeNames.TicketClosededName, Value = ((byte)EmailTemplateType.TicketClosed).ToString(CultureInfo.InvariantCulture) });
            collection.Add(new SelectListItem { Text = EmailTemplateTypeNames.NewMessageName, Value = ((byte)EmailTemplateType.NewMessage).ToString(CultureInfo.InvariantCulture) });
            collection.Add(new SelectListItem { Text = EmailTemplateTypeNames.TicketAssignedToUserName, Value = ((byte)EmailTemplateType.TicketAssignedToUser).ToString(CultureInfo.InvariantCulture) });
        }
    }
}