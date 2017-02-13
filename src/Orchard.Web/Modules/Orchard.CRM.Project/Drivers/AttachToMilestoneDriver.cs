using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Project.Models;
using Orchard.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using Orchard.Localization;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.Models;
using Newtonsoft.Json.Linq;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.ViewModels;
using System.Globalization;

namespace Orchard.CRM.Project.Drivers
{
    public class AttachToMilestoneDriver : MenuBaseDriver<AttachToMilestonePart>
    {
        private readonly IMilestoneService milestoneService;
        public AttachToMilestoneDriver(
            IMilestoneService milestoneService,
            ICRMContentOwnershipService contentOwnershipService,
            IExtendedProjectService projectService,
            IOrchardServices services,
            IHelperService helperService,
            IFolderService folderService)
            : base(contentOwnershipService, projectService, services, helperService, folderService)
        {
            this.milestoneService = milestoneService;
        }

        protected override DriverResult Display(AttachToMilestonePart part, string displayType, dynamic shapeHelper)
        {
            if (part.Record.MilestoneId.HasValue)
            {
                var milesone = this.milestoneService.GetMilestone(part.Record.MilestoneId.Value);

                if (milesone != null)
                {
                    dynamic model = new JObject();
                    model.Title = milesone.As<TitlePart>().Title;
                    model.Size = part.Record.Size;
                    model.MilestoneId = milesone.Id;

                    return ContentShape("Parts_AttachMilestone", () => shapeHelper.Parts_AttachMilestone(Model: model));
                }
            }

            return null;
        }

        protected override DriverResult Editor(AttachToMilestonePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (!this.contentOwnershipService.IsCurrentUserOperator())
            {
                return null;
            }

            // Check whether the given contentItem is a Ticket
            int? milestoneOfParentId = this.GetMilestoneOfParentId(part);

            if (milestoneOfParentId == null)
            {
                EditAttachToViewModel model = new EditAttachToViewModel();
                updater.TryUpdateModel(model, Prefix, null, null);

                part.Record.MilestoneId = model.SelectedId;

                part.Record.Size = model.Size;
            }
            else
            {
                part.Record.MilestoneId = milestoneOfParentId;
            }

            return this.Editor(part, shapeHelper);
        }

        protected int? GetProjectId(AttachToMilestonePart part)
        {
            int? projectId = null;

            // if it is Create Mode
            AttachToProjectPart attachToProjectPart = part.As<AttachToProjectPart>();
            if (attachToProjectPart == null || attachToProjectPart.Record.Project == null)
            {
                projectId = this.projectService.GetProjectIdFromQueryString();
            }
            else
            {
                projectId = attachToProjectPart.Record.Project != null ? (int?)attachToProjectPart.Record.Project.Id : null;
            }

            return projectId;
        }

        protected int? GetMilestoneOfParentId(AttachToMilestonePart part)
        {
            // Check whether the given contentItem is a Ticket
            int? milestoneOfParentId = null;
            TicketPart ticketPart = part.As<TicketPart>();
            if (ticketPart != null && ticketPart.Record.Parent != null)
            {
                // if the parent has a milestone, then the current ticket belongs to that milestone too
                var parent = this.services.ContentManager.Get(ticketPart.Record.Parent.Id, VersionOptions.Published, new QueryHints().ExpandParts<AttachToMilestonePart>());
                var parentAttachToMilestone = parent.As<AttachToMilestonePart>();
                milestoneOfParentId = parentAttachToMilestone.Record.MilestoneId;
            }

            return milestoneOfParentId;
        }

        protected override DriverResult Editor(AttachToMilestonePart part, dynamic shapeHelper)
        {
            if (!this.contentOwnershipService.IsCurrentUserOperator())
            {
                return null;
            }

            int? projectId = this.GetProjectId(part);

            if (projectId == null)
            {
                return null;
            }

            // Check whether the given contentItem is a Ticket
            int? milestoneOfParentId = this.GetMilestoneOfParentId(part);

            var milestones = this.milestoneService.GetOpenMilestones(projectId.Value);

            EditAttachToViewModel model = new EditAttachToViewModel();
            model.Size = part.Record.Size;
            Converter.Fill(model.Items, milestones, false);

            if (milestoneOfParentId.HasValue)
            {
                model.ParentId = milestoneOfParentId;
                var milestoneOfParent = milestones.FirstOrDefault(c => c.Id == milestoneOfParentId.Value);
                if (milestoneOfParent != null)
                {
                    model.ParentName = milestoneOfParent.As<TitlePart>().Title;
                }
            }

            if (part.Record.MilestoneId.HasValue)
            {
                var selectedItem = model.Items.FirstOrDefault(c => c.Value == part.Record.MilestoneId.Value.ToString(CultureInfo.InvariantCulture));
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }

            return ContentShape("Parts_AttachToMilestone_Edit",
                     () => shapeHelper.EditorTemplate(
                         TemplateName: "Parts/AttachToMilestone",
                         Model: model,
                         Prefix: Prefix));
        }
    }
}