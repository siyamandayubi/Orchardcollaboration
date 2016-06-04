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

namespace Orchard.CRM.Core
{
    using Orchard.ContentManagement.MetaData;
    using Orchard.Core.Contents.Extensions;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.Data;
    using Orchard.Data.Migration;
    using Orchard.Data.Migration.Schema;
    using System;
    using System.Linq;
    using System.Data;
    using Orchard.Environment.Configuration;

    public class Migrations : DataMigrationImpl
    {
        private readonly Lazy<ICRMSetup> crmSetup;
        private readonly IRepository<StatusRecord> statusRepository;
        private readonly ShellSettings shellSettings;

        public Migrations(Lazy<ICRMSetup> crmSetup, IRepository<StatusRecord> statusRepository, ShellSettings shellSettings)
        {
            this.shellSettings = shellSettings;
            this.crmSetup = crmSetup;
            this.statusRepository = statusRepository;
        }

        public int Create()
        {
            this.CreateFileUploadRecordAndPart();
            this.CreateBusinessUnitsAndTeam();
            this.CreateTicketAndCommentsAndRelatedData();
            this.CreateMenuRecordsAndParts();
            this.CreateActivityHistory();
            this.CreateProjectAndAttachToProjectTypes();

            ContentDefinitionManager.AlterPartDefinition("ProjectionWithDynamicSortPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("DashboardPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("TicketsForContentItemPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("TicketsDashboardWidget",
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("DashboardPart")
                    .WithPart("ActivityStreamPart")
                    .WithSetting("Stereotype", "Widget")
                    .DisplayedAs("Tickets Dashboard")
                );

            ContentDefinitionManager.AlterTypeDefinition("Dashboard",
                cfg => cfg
                   .WithPart("CommonPart")
                   .WithPart("TitlePart")
                   .WithPart("AutoroutePart")
                   .WithPart("MenuPart")
                   .WithPart("DashboardPart")
                   .WithPart("ActivityStreamPart")
                   .WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "5"))
                .Creatable()
                .DisplayedAs("Tickets Dashboard"));


            ContentDefinitionManager.AlterTypeDefinition("ProjectionWithDynamicSortPage",
                 cfg => cfg
                   .WithPart("CommonPart")
                   .WithPart("TitlePart")
                   .WithPart("AutoroutePart", builder => builder
                   .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                   .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                   .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-projections'}]")
                   .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                   .WithPart("MenuPart")
                   .WithPart("ProjectionWithDynamicSortPart")
                   .WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "5"))
                   .DisplayedAs("Projection With DynamicSort")
                );

            SchemaBuilder.CreateTable("EmailTemplateRecord", table => table
                          .Column<int>("Id", c => c.PrimaryKey().Identity())
                          .Column<string>("Name", c => c.WithLength(100).NotNull())
                          .Column<string>("Subject", c => c.WithLength(100).NotNull())
                          .Column<int>("TypeId", c => c.NotNull())
                          .Column<string>("Body", c => c.NotNull().WithLength(2000)));

            this.crmSetup.Value.AddBasicData();
            return 5;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTable("TicketPartRecord",
                        table => table.AddColumn<int>("SourceId", c => c.Nullable()));

            SchemaBuilder.AlterTable("TicketPartRecord",
                       table => table.AddColumn<string>("SourceData", c => c.Nullable().WithLength(100)));
            return 3;
        }

        public int UpdateFrom3()
        {
            this.CreateActivityHistory();
            ContentDefinitionManager.AlterTypeDefinition("Dashboard", c => c.WithPart("ActivityStreamPart"));
            ContentDefinitionManager.AlterTypeDefinition("TicketsDashboardWidget", c => c.WithPart("ActivityStreamPart"));

            SchemaBuilder.AlterTable("StatusRecord",
                        table => table.AddColumn<int>("StatusTypeId", c => c.Nullable()));

            SchemaBuilder.AlterTable("StatusRecord",
                        table => table.AddColumn<bool>("Deleted", c => c.WithDefault(false).NotNull()));

            var statusRecords = this.statusRepository.Table.OrderBy(c => c.Id).ToList();

            // update new status
            var newStatus = statusRecords.FirstOrDefault(c => c.Name == "New");
            newStatus = newStatus ?? statusRecords[0];
            newStatus.StatusTypeId = StatusRecord.NewStatus;
            this.statusRepository.Update(newStatus);

            // update open Status
            var openStatus = statusRecords.FirstOrDefault(c => c.Name == "Open");
            openStatus = openStatus ?? statusRecords[1];
            openStatus.StatusTypeId = StatusRecord.OpenStatus;
            this.statusRepository.Update(openStatus);

            // update solved Status
            var solvedStatus = statusRecords.FirstOrDefault(c => c.Name == "Solved");
            solvedStatus = solvedStatus ?? statusRecords[2];
            solvedStatus.StatusTypeId = StatusRecord.DeferredStatus;
            this.statusRepository.Update(solvedStatus);

            // update closed Status
            var closedStatus = statusRecords.FirstOrDefault(c => c.Name == "Closed");
            closedStatus = closedStatus ?? statusRecords[3];
            closedStatus.StatusTypeId = StatusRecord.ClosedStatus;
            this.statusRepository.Update(closedStatus);

            this.statusRepository.Flush();
            return 4;
        }

        public int UpdateFrom4()
        {
            this.CreateProjectAndAttachToProjectTypes();

            SchemaBuilder.AlterTable("FileUploadPartRecord", table =>
                table.AddColumn<int>("FilesCount", c => c.Nullable().WithDefault(0)));

            return 5;
        }

        public int UpdateFrom5()
        {
            // status records
            var statusRecords = this.statusRepository.Table.OrderBy(c => c.Id).ToList();

            // rename open status to In Progress
            var openStatus = statusRecords.FirstOrDefault(c => c.StatusTypeId == StatusRecord.OpenStatus);
            if (openStatus != null)
            {
                openStatus.Name = "In Progress";
                this.statusRepository.Update(openStatus);
            }

            // rename Solved to Deferred
            var solvedStatus = statusRecords.FirstOrDefault(c => c.StatusTypeId == StatusRecord.DeferredStatus);
            if (solvedStatus != null)
            {
                solvedStatus.Name = "Deferred";
                this.statusRepository.Update(solvedStatus);
            }

            // rename Closed to Completed
            var closedStatus = statusRecords.FirstOrDefault(c => c.StatusTypeId == StatusRecord.ClosedStatus);
            if (closedStatus != null)
            {
                closedStatus.Name = "Completed";
                this.statusRepository.Update(closedStatus);
            }

            // pending input
            if (!statusRecords.Any(c => c.StatusTypeId == StatusRecord.PendingInputStatus))
            {
                StatusRecord pendingStatus = new StatusRecord
                {
                    StatusTypeId = StatusRecord.PendingInputStatus,
                    Name = "Pending input",
                    OrderId = 3,
                };
                this.statusRepository.Create(pendingStatus);
            }

            this.statusRepository.Flush();

            return 6;
        }

        private void CreateActivityHistory()
        {
            // Create TeamPartRecord table
            SchemaBuilder.CreateTable("ActivityStreamRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<int>("RelatedContent_Id", c => c.NotNull())
                .Column<int>("RelatedVersion_Id", c => c.NotNull())
                .Column<int>("User_Id", c => c.Nullable())
                .Column<string>("Description", c => c.Unlimited())
                .Column<DateTime>("CreationDateTime", c => c.NotNull())
                );

            ContentDefinitionManager.AlterPartDefinition("ActivityStreamPart", builder => builder.Attachable());

            // Activity Stream index
            SchemaBuilder.AlterTable("ActivityStreamRecord", table => table.CreateIndex(
                "ActivityStreamRecord_MainIndex",
                new string[] 
                { 
                    "RelatedVersion_Id",
                    "RelatedContent_Id",
                    "User_Id",
                }));
        }

        private void CreateFileUploadRecordAndPart()
        {
            ContentDefinitionManager.AlterPartDefinition("FileUploadPart", part =>
                part
                .WithSetting("FileUploadPartSettings.FileCountLimit", "10")
                .WithSetting("FileUploadPartSettings.FileSizeLimit", "10")
                .WithSetting("FileUploadPartSettings.FileTypes", "zip,rar,jpg,jpeg,png,gif,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,rtf,xps")
                .WithSetting("FileUploadPartSettings.PublicMediaPath", "{Content.Slug}")
                .Attachable());

            SchemaBuilder.CreateTable("FileUploadPartRecord", table => table
                                                                           .ContentPartRecord()
                                                                           .Column("FolderGuid", DbType.Guid)
                                                                           .Column<int>("FilesCount", c => c.Nullable().WithDefault(0)));

            if (this.shellSettings.DataProvider == "MySql")
            {
                string tableName = String.Concat(SchemaBuilder.FormatPrefix(SchemaBuilder.FeaturePrefix), "FileUploadPartRecord");
                SchemaBuilder.ExecuteSql(string.Format("ALTER TABLE {0} CONVERT TO CHARACTER SET binary", tableName), null);
            }

            // FileUploadPartRecord index
            SchemaBuilder.AlterTable("FileUploadPartRecord", table => table.CreateIndex(
                "FileUploadPartRecord_MainIndex",
                new string[] { "FolderGuid" }));
        }

        private void CreateMenuRecordsAndParts()
        {
            // CRMCommentPartRecord
            SchemaBuilder.CreateTable("TicketMenuItemPartRecord",
              table => table
                  .ContentPartRecord()
                  .Column<int>("Status_Id")
                  .Column<int>("BusinessUnit_Id")
                  .Column<int>("Team_Id")
                  .Column<int>("User_Id")
                  .Column<int>("DueDateDays")
           );

            ContentDefinitionManager.AlterPartDefinition("TicketMenuItemPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("TicketMenuItem", cfg => cfg
                .WithPart("MenuPart")
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("TicketMenuItemPart")
                .DisplayedAs("Ticket Menu Item")
                .WithSetting("Description", "Add menu to the Ticket Search items")
                .WithSetting("Stereotype", "MenuItem"));
        }

        private void CreateTicketAndCommentsAndRelatedData()
        {
            // CRMCommentPartRecord
            SchemaBuilder.CreateTable("CRMCommentPartRecord",
              table => table
                  .ContentPartRecord()
                  .Column<int>("User_Id")
                  .Column<int>("CRMCommentsPartRecord_Id")
                  .Column<DateTime>("CommentDateUtc")
                  .Column<string>("CommentText", column => column.Unlimited())
                  .Column<bool>("IsEmail", column => column.NotNull().WithDefault(false))
                  .Column<bool>("IsHtml", column => column.NotNull().WithDefault(false))
                  .Column<string>("CC", column => column.Nullable().WithLength(200))
                  .Column<string>("Subject", column => column.Nullable().WithLength(200))
                  .Column<string>("BCC", column => column.Nullable().WithLength(200))
                  .Column<string>("MTo", column => column.Nullable().WithLength(200))
                  .Column<string>("MFrom", column => column.Nullable().WithLength(200))
           );

            // CRMCommentPartRecord main index
            SchemaBuilder.AlterTable("CRMCommentPartRecord", table => table.CreateIndex(
                "CRMCommentPartRecord_MainIndex",
                new string[] 
                {
                    "User_Id",
                    "CRMCommentsPartRecord_Id",
                    "CommentDateUtc"
                }));

            // CRMCommentsPartRecord
            SchemaBuilder.CreateTable("CRMCommentsPartRecord", table => table
               .ContentPartRecord()
               .Column<bool>("ThreadedComments")
               .Column<int>("CommentsCount")
               );

            ContentDefinitionManager.AlterPartDefinition("CRMCommentPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("CRMCommentsPart", builder => builder.Attachable());

            // Create ServiceRecord table
            SchemaBuilder.CreateTable("ServiceRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Name", c => c.WithLength(50))
                .Column<string>("Description", c => c.WithLength(500))
                .Column<bool>("Deleted", c => c.WithDefault(false).NotNull())
                );

            // Create Priority table
            SchemaBuilder.CreateTable("PriorityRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Name", c => c.WithLength(50).Nullable())
                .Column<bool>("IsHardCode", c => c.WithDefault(false))
                .Column<int>("OrderId", c => c.Nullable())
                );

            // Create TicketType table
            SchemaBuilder.CreateTable("TicketTypeRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Name", c => c.WithLength(50).Nullable())
                .Column<bool>("IsHardCode", c => c.WithDefault(false))
                .Column<int>("OrderId", c => c.Nullable())
                .Column<bool>("Deleted", c => c.WithDefault(false).NotNull())
                );

            // Create Status table
            SchemaBuilder.CreateTable("StatusRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Name", c => c.WithLength(50).Nullable())
                .Column<int>("StatusTypeId", c => c.Nullable())
                .Column<bool>("IsHardCode", c => c.WithDefault(false))
                .Column<int>("OrderId", c => c.Nullable())
                .Column<bool>("Deleted", c => c.WithDefault(false).NotNull())
                );

            SchemaBuilder.CreateTable("TicketIdentityRecord", table =>
               table.Column<int>("Id", c => c.Identity().PrimaryKey())
               .Column<string>("TempData", c => c.WithLength(100))
               );

            // Create TicketPartRecord table
            SchemaBuilder.CreateTable("TicketPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Title", c => c.WithLength(100))
                .Column<int>("RelatedContentItem_Id", c => c.Nullable())
                .Column<int>("Parent_Id", c => c.Nullable())
                .Column<int>("SourceId", c => c.Nullable())
                .Column<string>("SourceData", c => c.Nullable().WithLength(100))
                .Column<int>("RequestingUser_Id", c => c.Nullable())
                .Column<int>("TicketType_Id", c => c.Nullable())
                .Column<int>("PriorityRecord_Id", c => c.Nullable())
                .Column<int>("Identity_Id", c => c.Nullable())
                .Column<DateTime>("DueDate", c => c.Nullable())
                .Column<int>("Service_Id", c => c.Nullable())
                .Column<int>("StatusRecord_Id", c => c.Nullable())
                .Column<string>("Description", c => c.Unlimited())
                );

            // Identity index
            SchemaBuilder.AlterTable("TicketPartRecord", table => table.CreateIndex(
                "Ticket_IdentityIndex",
                new string[] { "Identity_Id" }));

            // RelatedContentItem index
            SchemaBuilder.AlterTable("TicketPartRecord", table => table.CreateIndex(
                "Ticket_RelatedContentItem_IdIndex",
                new string[] { "RelatedContentItem_Id" }));

            // Parent index
            SchemaBuilder.AlterTable("TicketPartRecord", table => table.CreateIndex(
                "Ticket_ParentIndex",
                new string[] { "Parent_Id" }));

            // main index
            SchemaBuilder.AlterTable("TicketPartRecord", table => table.CreateIndex(
                "Ticket_MainIndex",
                new string[] 
                {
                    "StatusRecord_Id",
                    "DueDate",
                    "TicketType_Id",
                    "PriorityRecord_Id",
                    "Service_Id",
                    "Parent_Id",
                    "RequestingUser_Id",
                    "RelatedContentItem_Id",
                    "Identity_Id"
                }));

            // ContentItemVersionRecord main index
            SchemaBuilder coreSchemaBuilder = new SchemaBuilder(this.SchemaBuilder.Interpreter, "Orchard_Framework_");
            coreSchemaBuilder.AlterTable(
              "ContentItemVersionRecord",
              table => table
                .CreateIndex("ContentItemVersionRecord_ContentItemRecord_id",
                "ContentItemRecord_id"));

            // Create ContentItemPermissionDetailRecord table
            SchemaBuilder.CreateTable("ContentItemPermissionDetailRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<int>("Team_Id", c => c.Nullable())
                .Column<int>("BusinessUnit_Id", c => c.Nullable())
                .Column<int>("User_Id", c => c.Nullable())
                .Column<int>("ContentItemPermissionPartRecord_Id", c => c.Nullable())
                .Column<int>("Parent_Id", c => c.Nullable())
                 .Column<byte>("AccessType", c => c.Nullable())
               );

            // ContentItemPermissionDetailRecord main index
            SchemaBuilder.AlterTable("ContentItemPermissionDetailRecord", table => table.CreateIndex(
                "ContentItemPermissionDetailRecord_MainIndex",
                new string[] 
                {
                    "BusinessUnit_Id",
                    "User_Id",
                    "Team_Id",
                    "ContentItemPermissionPartRecord_Id",
                    "AccessType",
                    "Parent_Id"
                }));


            // Create ContentItemPermissionPartRecord table
            SchemaBuilder.CreateTable("ContentItemPermissionPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("Ticket_Id", c => c.Nullable())
                .Column<bool>("HasOwner", c => c.WithDefault(false))
                );

            // ContentItemPermissionPartRecord main index
            SchemaBuilder.AlterTable("ContentItemPermissionPartRecord", table => table.CreateIndex(
                "ContentItemPermissionPartRecord_MainIndex",
                new string[] 
                {
                    "Ticket_Id",
                    "HasOwner"
                }));

            ContentDefinitionManager.AlterPartDefinition("ContentItemPermissionPart",
                builder => builder.Attachable());

            ContentDefinitionManager.AlterPartDefinition("TicketPart",
               builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("Ticket",
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("TicketPart")
                .WithPart("ContentItemPermissionPart")
                .WithPart("IdentityPart")
                .WithPart("CRMCommentsPart")
                .WithPart("FileUploadPart")
                .DisplayedAs("Ticket"));

            ContentDefinitionManager.AlterTypeDefinition("CRMComment",
                   cfg => cfg
                    .WithPart("IdentityPart")
                    .WithPart("CommonPart")
                    .WithPart("CRMCommentPart")
                    .DisplayedAs("CRMComment"));
        }

        private void CreateProjectAndAttachToProjectTypes()
        {
            // Create ProjectPartRecord table
            SchemaBuilder.CreateTable("ProjectPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Title", c => c.WithLength(100).Nullable())
                .Column<string>("Description", c => c.Unlimited().Nullable())
                .Column<int>("Detail_Id", c => c.Nullable()));


            // Create AttachToProject table
            SchemaBuilder.CreateTable("AttachToProjectPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("Project_Id", c => c.Nullable()));

            ContentDefinitionManager.AlterPartDefinition("ProjectPart",
               builder => builder.Attachable());

            ContentDefinitionManager.AlterPartDefinition("AttachToProjectPart",
                 builder => builder.Attachable());

            // AttachToProjectPartRecord index
            SchemaBuilder.AlterTable("AttachToProjectPartRecord", table => table.CreateIndex(
                "AttachToProjectPartRecord_MainIndex",
                new string[] { "Project_Id" }));

        }

        private void CreateBusinessUnitsAndTeam()
        {
            // Create TeamPartRecord table
            SchemaBuilder.CreateTable("TeamPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Name", c => c.WithLength(50))
                .Column<string>("Description", c => c.WithLength(200))
                .Column<int>("BusinessUnitPartRecord_Id", c => c.Nullable())
                );

            // Create TeamMemberRecord table
            SchemaBuilder.CreateTable("TeamMemberPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("TeamPartRecord_Id", c => c.Nullable())
                .Column<int>("UserPartRecord_Id", c => c.Nullable())
                );

            // Create BusinessUnitMemberRecord table
            SchemaBuilder.CreateTable("BusinessUnitMemberPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("BusinessUnitPartRecord_Id", c => c.Nullable())
                .Column<int>("UserPartRecord_Id", c => c.Nullable())
                );

            // Create BusinessUnitPartRecord table
            SchemaBuilder.CreateTable("BusinessUnitPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Name", c => c.WithLength(50))
                .Column<string>("Description", c => c.WithLength(200))
                .Column<int>("Parent_Id", c => c.Nullable())
                );

            ContentDefinitionManager.AlterPartDefinition("TeamMemberPart",
               builder => builder.Attachable());

            ContentDefinitionManager.AlterPartDefinition("BusinessUnitMemberPart",
                   builder => builder.Attachable());

            ContentDefinitionManager.AlterPartDefinition("EntityAccessPart",
              builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("TeamMember",
                       cfg => cfg
                       .WithPart("CommonPart")
                        .WithPart("TeamMemberPart")
                        .DisplayedAs("TeamMember")
                        );

            ContentDefinitionManager.AlterTypeDefinition("BusinessUnitMember",
                       cfg => cfg
                       .WithPart("CommonPart")
                        .WithPart("BusinessUnitMemberPart")
                        .DisplayedAs("Business Unit Member")
                        );

            ContentDefinitionManager.AlterTypeDefinition("Team",
                 cfg => cfg
                     .WithPart("CommonPart")
                     .WithPart("TeamPart")
                     .DisplayedAs("Team")
                 );

            ContentDefinitionManager.AlterTypeDefinition("BusinessUnit",
                 cfg => cfg
                     .WithPart("CommonPart")
                     .WithPart("BusinessUnitPart")
                     .DisplayedAs("Business Unit")
                 );
        }
    }
}