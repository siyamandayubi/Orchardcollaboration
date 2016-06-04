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

using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.CRM.Core.Controllers;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using System;
using System.Data;

namespace Orchard.CRM.Project
{
    public class Migrations : DataMigrationImpl
    {
        private Lazy<IProjectSetup> projectSetup;
        private readonly IDataMigrationInterpreter dataMigrationInterpreter;
        public Migrations(Lazy<IProjectSetup> projectSetup, IDataMigrationInterpreter dataMigrationInterpreter)
        {
            this.dataMigrationInterpreter = dataMigrationInterpreter;
            this.projectSetup = projectSetup;
        }

        public int Create()
        {
            // Create AttachToFolder table
            SchemaBuilder.CreateTable("AttachToFolderPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("Folder_Id", c => c.Nullable()));

            ContentDefinitionManager.AlterPartDefinition("AttachToFolderPart",
                 builder => builder.Attachable());

            // Create Folder table
            SchemaBuilder.CreateTable("FolderPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("Parent_Id", c => c.Nullable())
                .Column<int>("Project_Id", c => c.Nullable())
                .Column<string>("Title", c => c.Nullable().WithLength(100)));

            ContentDefinitionManager.AlterPartDefinition("FolderPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("FollowerPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("FolderListPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("ProjectTitleAndMenuPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.FolderContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("FolderPart")
                .WithPart("IdentityPart")
                .WithPart("FolderListPart")
                .WithPart("AttachToProjectPart", c =>
                    c.WithSetting("AttachToProjectPartSettings.HiddenInEditMode", "true")
                     .WithSetting("AttachToProjectPartSettings.HiddenInDisplayModel", "true"))
                .WithPart("ContentItemPermissionPart")
                .WithPart("ProjectItemPermissionsPart")
                .DisplayedAs("Folder")
                .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectTicketsSummaryGroupByStatusContentType,
               cfg => cfg
               .WithPart("CommonPart")
               .WithPart("DataReportViewerPart")
               .WithPart("ContainablePart")
               .DisplayedAs(ContentTypes.ProjectTicketsSummaryGroupByStatusContentType)
               .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectTicketsContentType,
              cfg => cfg
              .WithPart("CommonPart")
              .WithPart("TitlePart")
              .WithPart("AttachToProjectPart")
              .WithPart("ProjectTitleAndMenuPart")
              .WithPart("ProjectionWithDynamicSortPart")
              .DisplayedAs(ContentTypes.ProjectTicketsContentType)
              .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.DiscussionsSummaryContentType,
               cfg => cfg
               .WithPart("CommonPart")
               .WithPart("ProjectionWithDynamicSortPart")
               .WithPart("ContainablePart")
               .DisplayedAs(ContentTypes.DiscussionsSummaryContentType)
               .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.TicketssSummaryContentType,
               cfg => cfg
               .WithPart("CommonPart")
               .WithPart("ProjectionWithDynamicSortPart")
               .WithPart("ContainablePart")
               .WithPart("TitlePart")
               .DisplayedAs(ContentTypes.TicketssSummaryContentType)
               .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.EmptyContentType,
                 cfg => cfg
                 .WithPart("CommonPart")
                 .WithPart("TitlePart")
                 .WithPart("AutoroutePart")
                 .DisplayedAs(ContentTypes.EmptyContentType)
                 .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectLastActivityStreamContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("ActivityStreamPart", c =>
                    c.WithSetting("ActivityStreamPartSettings.PageSize", "5"))
                .WithPart("ContainablePart")
                .DisplayedAs(ContentTypes.ProjectTicketsSummaryGroupByStatusContentType)
                .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.WikiItemType,
               cfg => cfg
               .WithPart("CommonPart")
               .WithPart("TitlePart")
               .WithPart("BodyPart")
               .WithPart("FollowerPart")
               .WithPart("ProjectItemPermissionsPart")
               .WithPart("CRMCommentsPart")
               .WithPart("FolderListPart")
               .WithPart("IdentityPart")
               .WithPart("AttachToProjectPart", c => c.WithSetting("AttachToProjectPartSettings.HiddenInEditMode", "true"))
               .WithPart("AttachToFolderPart")
               .WithPart("ContentItemPermissionPart")
               .WithPart("FileUploadPart", c =>
                    c.WithSetting("FileUploadPartSettings.DisplayFileUploadInDisplayMode", "true")
                     .WithSetting("FileUploadPartSettings.HideFileUploadInEditModel", "true")
                     .WithSetting("FileUploadPartSettings.FileTypes", "zip,rar,jpg,jpeg,png,gif,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,rtf,xps"))
              .WithSetting("TypeIndexing.Indexes", TicketController.SearchIndexName)
               .DisplayedAs("Wiki Item")
               .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectDetailContentType,
               cfg => cfg
               .WithPart("CommonPart")
               .WithPart("AttachToProjectPart", c =>
                   c.WithSetting("AttachToProjectPartSettings.HiddenInEditMode", "true")
                   .WithSetting("AttachToProjectPartSettings.HiddenInDisplayModel", "true"))
               .WithPart("ContainerPart")
               .DisplayedAs("Project Detail")
               .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectActivityStreamType,
              cfg => cfg
              .WithPart("CommonPart")
              .WithPart("AttachToProjectPart", c =>
                  c.WithSetting("AttachToProjectPartSettings.HiddenInEditMode", "true")
                  .WithSetting("AttachToProjectPartSettings.HiddenInDisplayModel", "true"))
              .WithPart("ActivityStreamPart")
              .WithPart("ProjectTitleAndMenuPart")
              .DisplayedAs("Project ActivityStream")
              .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition("Ticket",
                cfg => cfg.WithPart("AttachToProjectPart"));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectListContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("ProjectListPart")
                .WithPart("MenuPart")
                .DisplayedAs("Project List")
                .Creatable(true));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.WikiContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("AttachToProjectPart")
                .WithPart("ProjectTitleAndMenuPart")
                .WithPart("FolderListPart")
                .WithPart("ActivityStreamPart")
                .DisplayedAs("Project Wiki")
                .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.RootWikiContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("AttachToProjectPart")
                .WithPart("ProjectTitleAndMenuPart")
                .WithPart("RootFolderItemsPart")
                .WithPart("FolderListPart")
                .DisplayedAs("Root Wiki")
                .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("ProjectPart")
                .WithPart("ContentItemPermissionPart")
                .DisplayedAs("Project"));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.DiscussionContentType,
                cfg => cfg
                .WithPart("CommonPart", c => c.WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
                .WithPart("TitlePart")
                .WithPart("IdentityPart")
                .WithPart("BodyPart")
                .WithPart("FollowerPart")
                .WithPart("AttachToProjectPart", c =>
                    c.WithSetting("AttachToProjectPartSettings.HiddenInEditMode", "true")
                     .WithSetting("AttachToProjectPartSettings.HiddenInDisplayModel", "true")
                     .WithSetting("AttachToProjectPartSettings.IgnoreInActivityStream", "true"))
                .WithPart("ProjectTitleAndMenuPart")
                .WithPart("ContentItemPermissionPart")
                .WithPart("ProjectItemPermissionsPart")
                .WithPart("CRMCommentsPart")
                .WithPart("FileUploadPart", c =>
                   c.WithSetting("FileUploadPartSettings.DisplayFileUploadInDisplayMode", "true")
                   .WithSetting("FileUploadPartSettings.FileTypes", "zip,rar,jpg,jpeg,png,gif,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,rtf,xps")
                   .WithSetting("FileUploadPartSettings.HideFileUploadInEditModel", "true"))
                .WithSetting("TypeIndexing.Indexes", TicketController.SearchIndexName)
                .DisplayedAs("Discussion"));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectDiscussionsContentType,
               cfg => cfg
               .WithPart("CommonPart")
               .WithPart("TitlePart")
               .WithPart("ProjectTitleAndMenuPart")
               .WithPart("AttachToProjectPart")
               .WithPart("ProjectionWithDynamicSortPart")
               .DisplayedAs(ContentTypes.ProjectDiscussionsContentType)
               .Creatable(false));

            // Add file field to user
            ContentDefinitionManager.AlterPartDefinition("User",
                cfg =>
                    cfg.WithField(FieldNames.UserThumbnailImageField, c =>
                        c.OfType("FileField")
                        .WithDisplayName("Thumbnail Image")
                        .WithSetting("FileFieldSettings.ExtenstionsAllowed", ".jpg, .jpeg, .bmp, .png, .tif")
                        .WithSetting("FileFieldSettings.MaxFileSize", "1024"))
                    .WithField(FieldNames.UserTelField, c => c.WithDisplayName("Tel").OfType("InputField"))
                    .WithField(FieldNames.UserTags, c => c.WithDisplayName("Tags").OfType("InputField").WithSetting("InputFieldSettings.Hint", "Comma Seperated Tags"))
                    .WithField(FieldNames.UserSkypeIdField, c => c.WithDisplayName("Skype Id").OfType("InputField"))
                    .WithField(FieldNames.UserMobileField, c => c.WithDisplayName("Mobile:").OfType("InputField"))
                );

            // FolderPartRecord index
            SchemaBuilder.AlterTable("FolderPartRecord", table => table.CreateIndex(
                "FolderPartRecord_MainIndex",
                new string[] { "Project_Id", "Parent_Id" }));

            // AttachToFolderPartRecord index
            SchemaBuilder.AlterTable("AttachToFolderPartRecord", table => table.CreateIndex(
                "AttachToFolderPartRecord_MainIndex",
                new string[] { "Folder_Id" }));

            var orchardProjectionModuleBuilder = new SchemaBuilder(this.dataMigrationInterpreter, "Orchard_Projections_");

            this.projectSetup.Value.Setup();

            this.UpdateFrom1();
            this.CreateMilestoneAndProjectMenuTypes();
            return 4;
        }

        public int UpdateFrom1()
        {
            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectDashboardProjectionPortletContentType,
               cfg => cfg
               .WithPart("CommonPart")
               .WithPart("TitlePart")
               .WithPart("ProjectionWithDynamicSortPart")
               .WithPart("ContainablePart")
               .DisplayedAs("Project-Dashboard Projection Protlet"));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectDashboardProjectionPortletTemplateContentType,
                 cfg => cfg
                 .WithPart("CommonPart")
                 .WithPart("TitlePart")
                 .WithPart("ProjectionWithDynamicSortPart")
                 .WithPart("ContainablePart")
                 .DisplayedAs("Project-Dashboard Projection Protlet Template")
                 .Creatable(true)
                 .Listable(true));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectDashboardReportViewerPortletContentType,
               cfg => cfg
               .WithPart("CommonPart")
               .WithPart("TitlePart")
               .WithPart("DataReportViewerPart")
               .WithPart("ContainablePart")
               .DisplayedAs("Project-Dashboard Summary Portlet")
               .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectDashboardReportViewerPortletTemplateContentType,
                cfg => cfg
               .WithPart("CommonPart")
               .WithPart("TitlePart")
               .WithPart("DataReportViewerPart")
               .WithPart("ContainablePart")
               .DisplayedAs("Project-Dashboard Summary Portlet Template")
               .Creatable(true)
               .Listable(true));

            ContentDefinitionManager.AlterPartDefinition("ProjectDashboardEditorPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectContentType,
              cfg => cfg
              .WithPart("ProjectDashboardEditorPart"));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectLastActivityStreamContentType,
                cfg => cfg
                    .WithPart("TitlePart"));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectLastActivityStreamTemplateContentType,
                cfg => cfg
                    .WithPart("TitlePart")
                    .WithPart("CommonPart")
                    .WithPart("ActivityStreamPart", c =>
                        c.WithSetting("ActivityStreamPartSettings.PageSize", "5"))
                        .WithPart("ContainablePart")
                        .DisplayedAs(ContentTypes.ProjectLastActivityStreamTemplateContentType)
                        .Creatable(false));

            this.projectSetup.Value.Setup2();
            return 3;
        }

        public int UpdateFrom3()
        {
            this.CreateMilestoneAndProjectMenuTypes();
            this.projectSetup.Value.Setup3();
            return 4;
        }

        public int UpdateFrom4()
        {
            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectProjectionContentType, cfg => cfg.Listable(true).Creatable(true));
            return 5;
        }

        private void CreateMilestoneAndProjectMenuTypes()
        {
            // MilestonePartRecord
            SchemaBuilder.CreateTable("MilestonePartRecord", table => table
                .ContentPartRecord()
                .Column<DateTime>("StartTime", c => c.Nullable())
                .Column<DateTime>("EndTime", c => c.Nullable())
                .Column<bool>("IsCurrent", c => c.Nullable())
                .Column<bool>("IsClosed", c => c.Nullable())
                .Column<bool>("IsBacklog", c => c.Nullable()));

            // AttachToMilestonePartRecord
            SchemaBuilder.CreateTable("AttachToMilestonePartRecord", table => table
                .ContentPartRecord()
                .Column<int>("MilestoneId", c => c.Nullable())
                .Column<int>("Size", c => c.Nullable())
                .Column<int>("OrderId", c => c.Nullable()));

            // AttachToMilestonePartRecord index
            SchemaBuilder.AlterTable("AttachToMilestonePartRecord", table => table.CreateIndex(
                "AttachToMilestonePartRecord_MainIndex",
                new string[] { "MilestoneId", "OrderId" }));

            // MilestonePart
            ContentDefinitionManager.AlterPartDefinition("MilestonePart", builder => builder.Attachable());
            
            // AttachToMilestonePart
            ContentDefinitionManager.AlterPartDefinition("AttachToMilestonePart", builder => builder.Attachable());

            // Attach Milestone to Ticket
            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.TicketContentType, cfg => cfg.WithPart("AttachToMilestonePart"));

            // Milestone
            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.MilestoneContentType,                
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("IdentityPart")
                .WithPart("ProjectTitleAndMenuPart")
                .WithPart("MilestonePart")
                .WithPart("ContentItemPermissionPart")
                .WithPart("ProjectItemPermissionsPart")
                .WithPart("AttachToProjectPart", c =>
                    c.WithSetting("AttachToProjectPartSettings.HiddenInEditMode", "true")
                     .WithSetting("AttachToProjectPartSettings.HiddenInDisplayModel", "true"))
                .DisplayedAs("Milestone"));

            // Project Menu
            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectMenuContentType, cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("AttachToProjectPart")
                .WithPart("MenuWidgetPart"));

            // Specify the type of the items so that in the view we build a url for creation new items
            ContentDefinitionManager.AlterPartDefinition(ContentTypes.ProjectProjectionContentType,
                cfg => cfg.WithField(
                    FieldNames.ProjectProjectionItemTypeFieldName, 
                    c => c.WithDisplayName("Item Type").OfType("InputField"))
                    .WithField(
                    FieldNames.ProjectProjectionItemTypeDisplayFieldName, 
                    c => c.WithDisplayName("Item Type Display Name").OfType("InputField")));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectProjectionContentType,
               cfg => cfg
               .WithPart("CommonPart")
               .WithPart("TitlePart")
               .WithPart(ContentTypes.ProjectProjectionContentType)
               .WithPart("ProjectTitleAndMenuPart")
               .WithPart("AttachToProjectPart")
               .WithPart("ProjectionWithDynamicSortPart")
               .DisplayedAs(ContentTypes.ProjectProjectionContentType)
               .Creatable(false));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.ProjectActivityStreamType,
             cfg => cfg.WithPart("TitlePart"));
        }
    }
}