using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.Settings;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.ActivityStream
{
    public class AttachToProjectStreamWriter : IActivityStreamWriter
    {
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;
        private readonly IProjectService projectService;
        private readonly ICRMContentOwnershipService contentOwnershipService;

        public AttachToProjectStreamWriter(IContentItemDescriptorManager contentItemDescriptorManager, IProjectService projectService, ICRMContentOwnershipService contentOwnershipService)
        {
            this.contentOwnershipService = contentOwnershipService;
            this.projectService = projectService;
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            var attachToProject = context.ContentItem.As<AttachToProjectPart>();
            bool result = attachToProject != null && (context.Snapshot != null);

            if (result)
            {
                var settings = attachToProject.TypePartDefinition.Settings.GetModel<AttachToProjectPartSettings>();
                result = result && !settings.IgnoreInActivityStream;
            }

            if (result && attachToProject.Record.Project != null)
            {
                var project = this.projectService.GetProject(attachToProject.Record.Project.Id);

                // means user doesn't have access to it
                if (project == null)
                {
                    return false;
                }
            }

            return result;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                yield break;
            }

            var attachToProjectPart = context.ContentItem.As<AttachToProjectPart>();

            ProjectPartRecord newProject = attachToProjectPart.Record.Project;
            ProjectPartRecord oldProject = context.Snapshot != null ? context.Snapshot.Project : null;
            if (oldProject == null && newProject == null)
            {
                yield break;
            }
            else if (oldProject == null && newProject != null)
            {
                newProject = this.projectService.GetProject(newProject.Id).As<ProjectPart>().Record;
                string change = T("{0} is attached to project '{1}'", contentItemDescriptorManager.GetDescription(context.ContentItem), newProject.Title).Text;
                yield return new ActivityStreamChangeItem(change);
            }
            else if (oldProject != null && newProject == null)
            {
                oldProject = this.projectService.GetProject(oldProject.Id).As<ProjectPart>().Record;
                string change = T("{0} is detached from the project '{1}'", contentItemDescriptorManager.GetDescription(context.ContentItem), oldProject.Title).Text;
                yield return new ActivityStreamChangeItem(change);
            }
            else if (oldProject != null && newProject != null && oldProject.Id != newProject.Id)
            {
                var oldProjectContentItem = this.projectService.GetProject(oldProject.Id);
                var newProjectContentItem = this.projectService.GetProject(newProject.Id);

                string change = T("{0} is detached from the project '{1}'", contentItemDescriptorManager.GetDescription(context.ContentItem), oldProjectContentItem.Record.Title).Text;
                yield return new ActivityStreamChangeItem(change, true, oldProjectContentItem.ContentItem.Id, oldProjectContentItem.ContentItem.VersionRecord.Id);

                change = T("{0} is attached to project '{1}'", contentItemDescriptorManager.GetDescription(context.ContentItem), newProjectContentItem.Record.Title).Text;
                yield return new ActivityStreamChangeItem(change, true, newProjectContentItem.ContentItem.Id, newProjectContentItem.ContentItem.VersionRecord.Id);
            }
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            return null;
        }

        public string Name
        {
            get { return "AttachToProject"; }
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            var part = contentItem.As<AttachToProjectPart>();

            if (part == null)
            {
                return null;
            }

            var settings = part.TypePartDefinition.Settings.GetModel<AttachToProjectPartSettings>();
            if (settings.IgnoreInActivityStream)
            {
                return null;
            }

            dynamic oldData = new ExpandoObject();
            var oldDataDictionary = oldData as IDictionary<string, object>;

            if (part.Record.Project != null)
            {
                var project = this.projectService.GetProject(part.Record.Project.Id);

                if (project == null)
                {
                    return null;
                }

                oldData.Project = new ProjectPartRecord
                {
                    Id = project.Record.Id,
                    Description = project.Record.Description,
                    Title = project.Record.Title,
                    ContentItemRecord = new ContentItemRecord { Id = project.Record.ContentItemRecord.Id }
                };
            }
            else
            {
                oldData.Project = null;
            }

            oldData.VersionId = contentItem.VersionRecord.Id;
            oldData.ContentItemId = part.ContentItem.Id;
            return oldData;
        }
    }
}