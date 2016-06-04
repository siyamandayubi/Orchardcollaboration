/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

namespace Orchard.CRM.Core.Controllers
{
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.UI.Admin;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    [ValidateInput(false), Admin]
    public class BasicDataController : Controller
    {
        private readonly IRepository<PriorityRecord> priorityRepository;
        private readonly ITransactionManager transactionManager;
        private readonly IRepository<TicketTypeRecord> ticketTypeRepository;
        private readonly IRepository<StatusRecord> statusRepository;
        private readonly IRepository<ServiceRecord> serviceRepository;
        private readonly IOrchardServices services;
        private readonly IBasicDataService basicDataService;
        private readonly IValidationService validationService;

        public Localizer T { get; set; }

        public BasicDataController(
            IValidationService validationService,
            IRepository<StatusRecord> statusRepository,
            IRepository<PriorityRecord> priorityRepository,
            IRepository<ServiceRecord> serviceRepository,
            IRepository<TicketTypeRecord> ticketTypeRepository,
            IBasicDataService basicDataService,
            ITransactionManager transactionManager,
            IOrchardServices services)
        {
            this.transactionManager = transactionManager;
            this.validationService = validationService;
            this.statusRepository = statusRepository;
            this.basicDataService = basicDataService;
            this.T = NullLocalizer.Instance;
            this.services = services;
            this.serviceRepository = serviceRepository;
            this.ticketTypeRepository = ticketTypeRepository;
            this.priorityRepository = priorityRepository;
        }

        public ActionResult Priorities()
        {
            var priorities = this.priorityRepository.Table.ToList();

            var model = priorities.Select(c => new PriorityViewModel
            {
                PriorityId = c.Id,
                Name = c.Name,
                OrderId = c.OrderId
            });

            return this.View(model);
        }

        public ActionResult TicketStatusList()
        {
            var model = this.GetStatusList();

            return this.View(model);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Services()
        {
            var services = this.serviceRepository.Table.Where(c => c.Deleted == false).ToList();

            var model = services.OrderBy(c => c.Name).Select(c => new ServiceViewModel
            {
                ServiceId = c.Id,
                Name = c.Name,
                Description = c.Description
            });

            return this.View(model);
        }

        public ActionResult TicketTypes()
        {
            var ticketTypes = this.ticketTypeRepository.Table.Where(c => c.Deleted == false).ToList();

            var model = ticketTypes.Select(c => new TicketTypeViewModel
            {
                TicketTypeId = c.Id,
                Name = c.Name,
                OrderId = c.OrderId
            });

            return this.View(model);
        }

        public ActionResult CreatePriority()
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            return this.View(new PriorityViewModel());
        }

        public ActionResult CreateStatus()
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            return this.View(new StatusViewModel());
        }

        public ActionResult CreateService()
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            return this.View(new ServiceViewModel());
        }

        public ActionResult CreateTicketType()
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            return this.View(new TicketTypeViewModel());
        }

        public ActionResult EditPriority(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var priority = this.priorityRepository.Table.FirstOrDefault(c => c.Id == id);

            var model = new PriorityViewModel
            {
                PriorityId = priority.Id,
                Name = priority.Name,
                OrderId = priority.OrderId
            };

            return this.View(model);
        }

        public ActionResult EditStatus(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var status = this.statusRepository.Table.FirstOrDefault(c => c.Id == id);

            var model = new StatusViewModel
            {
                StatusId = status.Id,
                Name = status.Name,
                OrderId = status.OrderId,
                StatusTypeId = status.StatusTypeId
            };

            return this.View(model);
        }

        public ActionResult EditService(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var service = this.serviceRepository.Table.FirstOrDefault(c => c.Id == id);

            var model = new ServiceViewModel
            {
                ServiceId = service.Id,
                Name = service.Name,
                Description = service.Description
            };

            return this.View(model);
        }

        public ActionResult EditTicketType(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var ticketType = this.ticketTypeRepository.Table.FirstOrDefault(c => c.Id == id);

            var model = new TicketTypeViewModel
            {
                TicketTypeId = ticketType.Id,
                Name = ticketType.Name,
                OrderId = ticketType.OrderId
            };

            return this.View(model);
        }

        [HttpPost]
        public ActionResult EditPriorityPost(PriorityViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("EditPriority", model);
            }

            var priority = this.priorityRepository.Table.FirstOrDefault(c => c.Id == model.PriorityId);

            if (priority == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no priority with the given Id").ToString());
                return this.View("EditPriority", model);
            }

            priority.Name = model.Name;
            priority.OrderId = model.OrderId;
            this.priorityRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("Priorities");
        }

        [HttpPost]
        public ActionResult EditStatusPost(StatusViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            this.transactionManager.Demand();

            if (!this.ModelState.IsValid)
            {
                this.transactionManager.Cancel();
                return this.View("EditStatus", model);
            }

            var statusRecords = this.statusRepository.Table.ToList();
            var status = statusRecords.FirstOrDefault(c => c.Id == model.StatusId);

            if (status == null)
            {
                this.transactionManager.Cancel();
                this.ModelState.AddModelError("Id", this.T("There is no status with the given Id").ToString());
                return this.View("EditStatus", model);
            }

            // update status of old records
            statusRecords.Where(c => c.StatusTypeId == model.StatusTypeId && c.Id != model.StatusId).ToList().ForEach(c =>
            {
                c.StatusTypeId = null;
                this.statusRepository.Update(c);
            });

            if (!this.validationService.IsStatusTypeChangeValid(new StatusRecord
            {
                Id = model.StatusId,
                StatusTypeId = model.StatusTypeId,
                Name = model.Name,
                OrderId = model.OrderId
            },
                this.ModelState))
            {
                this.transactionManager.Cancel();
                return this.View("EditStatus", model);
            }

            status.Name = model.Name;
            status.OrderId = model.OrderId;
            status.StatusTypeId = model.StatusTypeId;


            this.statusRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("TicketStatusList");
        }

        [HttpPost]
        public ActionResult EditServicePost(ServiceViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("EditService", model);
            }

            var service = this.serviceRepository.Table.FirstOrDefault(c => c.Id == model.ServiceId);

            if (service == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no service with the given Id").ToString());
                return this.View("EditService", model);
            }

            service.Name = model.Name;
            service.Description = model.Description;
            this.serviceRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("Services");
        }

        [HttpPost]
        public ActionResult EditTicketTypePost(TicketTypeViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("EditTicketType", model);
            }

            var ticketType = this.ticketTypeRepository.Table.FirstOrDefault(c => c.Id == model.TicketTypeId);

            if (ticketType == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no ticketType with the given Id").ToString());
                return this.View("EditTicketType", model);
            }

            ticketType.Name = model.Name;
            ticketType.OrderId = model.OrderId;
            this.ticketTypeRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("TicketTypes");
        }

        public ActionResult RemovePriority(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var priority = this.priorityRepository.Table.FirstOrDefault(c => c.Id == id);

            if (priority == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no priority with the given Id").ToString());
                return this.View("Priorities", new PriorityViewModel
                {
                    PriorityId = priority.Id,
                    Name = priority.Name,
                    OrderId = priority.OrderId
                });
            }

            this.priorityRepository.Delete(priority);
            this.priorityRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("Priorities");
        }

        public ActionResult RemoveStatus(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var status = this.statusRepository.Table.FirstOrDefault(c => c.Id == id);

            if (status == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no status with the given Id").ToString());
                return this.View("TicketStatusList", this.GetStatusList());
            }

            if (!this.validationService.IsStatusTypeChangeValid(
                new StatusRecord
                {
                    StatusTypeId = null,
                    Id = status.Id,
                    Name = status.Name,
                    OrderId = status.OrderId
                },
                    this.ModelState))
            {
                return this.View("TicketStatusList", this.GetStatusList());
            }

            status.Deleted = true;
            this.statusRepository.Update(status);
            this.statusRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("TicketStatusList");
        }

        public ActionResult RemoveService(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var service = this.serviceRepository.Table.FirstOrDefault(c => c.Id == id);

            if (service == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no service with the given Id").ToString());
                return this.View("Services", new ServiceViewModel
                {
                    ServiceId = service.Id,
                    Name = service.Name,
                    Description = service.Description
                });
            }

            service.Deleted = true;
            this.serviceRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("Priorities");
        }

        public ActionResult RemoveTicketType(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var ticketType = this.ticketTypeRepository.Table.FirstOrDefault(c => c.Id == id);

            if (ticketType == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no ticketType with the given Id").ToString());
                return this.View("TicketTypes", new TicketTypeViewModel
                {
                    TicketTypeId = ticketType.Id,
                    Name = ticketType.Name,
                    OrderId = ticketType.OrderId
                });
            }

            ticketType.Deleted = true;
            this.ticketTypeRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("TicketTypes");
        }

        [HttpPost]
        public ActionResult CreatePriorityPost(PriorityViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("CreatePriority", model);
            }

            PriorityRecord priority = new PriorityRecord();
            this.priorityRepository.Create(priority);

            priority.Name = model.Name;
            priority.OrderId = model.OrderId;
            this.priorityRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("Priorities");
        }

        [HttpPost]
        public ActionResult CreateStatusPost(StatusViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            this.transactionManager.Demand();

            if (!this.ModelState.IsValid)
            {
                this.transactionManager.Cancel();
                return this.View("CreateStatus", model);
            }

            // update status of old records
            var statusRecords = this.statusRepository.Table.ToList();
            statusRecords.Where(c => c.StatusTypeId == model.StatusTypeId && c.Id != model.StatusId).ToList().ForEach(c =>
            {
                c.StatusTypeId = null;
                this.statusRepository.Update(c);
            });

            StatusRecord status = new StatusRecord();
            status.Name = model.Name;
            status.OrderId = model.OrderId;
            status.StatusTypeId = model.StatusTypeId;

            if (!this.validationService.IsStatusTypeChangeValid(status, this.ModelState))
            {
                this.transactionManager.Cancel();
                return this.View("CreateStatus", model);
            }

            this.statusRepository.Create(status);
            this.statusRepository.Flush();
            this.basicDataService.ClearCache();
            return RedirectToAction("TicketStatusList");
        }

        [HttpPost]
        public ActionResult CreateServicePost(ServiceViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("CreateService", model);
            }

            ServiceRecord service = new ServiceRecord();
            this.serviceRepository.Create(service);

            service.Name = model.Name;
            service.Description = model.Description;
            this.serviceRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("Services");
        }

        [HttpPost]
        public ActionResult CreateTicketTypePost(TicketTypeViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("CreateTicketType", model);
            }

            TicketTypeRecord ticketType = new TicketTypeRecord();
            this.ticketTypeRepository.Create(ticketType);

            ticketType.Name = model.Name;
            ticketType.OrderId = model.OrderId;
            this.ticketTypeRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("TicketTypes");
        }

        private IEnumerable<StatusViewModel> GetStatusList()
        {
            var statusRecords = this.statusRepository.Table.Where(c => c.Deleted == false).OrderBy(c => c.OrderId).ToList();

            var model = statusRecords.Select(c => new StatusViewModel
            {
                StatusId = c.Id,
                StatusTypeId = c.StatusTypeId,
                Name = c.Name,
                OrderId = c.OrderId
            });

            return model;
        }
    }
}