using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Records;
using System.Xml.Linq;
using Orchard.CRM.Dashboard.Models;
using System.Globalization;
using Orchard.Core.Common.Models;

namespace Orchard.CRM.Dashboard
{
    public class Migrations : DataMigrationImpl
    {
        private readonly Lazy<IRepository<ContentTypeRecord>> contentTypeRecordRepository;
        private readonly Lazy<IContentManager> contentManager;
        private readonly Lazy<IRepository<ContentItemRecord>> contentItemRecordRepository;

        public Migrations(
            Lazy<IRepository<ContentTypeRecord>> contentTypeRecordRepository,
            Lazy<IRepository<ContentItemRecord>> contentItemRecordRepository,
            Lazy<IContentManager> contentManager)
        {
            this.contentManager = contentManager;
            this.contentTypeRecordRepository = contentTypeRecordRepository;
            this.contentItemRecordRepository = contentItemRecordRepository;
        }

        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("DashboardPortletPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("GenericDashboardPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("IMapEmailPortletPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("SmtpEmailPortletPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("BasicDataPortletPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("NavigationPortletPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("WorkflowPortletPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("QueriesPortletPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("SidebarDashboardPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("ContentManagementPortletPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("CoverWidgetPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("DashboardManagementPart", builder => builder.Attachable());           

            ContentDefinitionManager.AlterTypeDefinition(Consts.ContentManagementPortletContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("ContentManagementPortletPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletPart")
                .DisplayedAs(Consts.ContentManagementPortletContentType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.QueriesPortletContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("QueriesPortletPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletPart")
                .DisplayedAs(Consts.SmtpPortletContentType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.SmtpPortletContentType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("SmtpEmailPortletPart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletPart")
                    .DisplayedAs(Consts.SmtpPortletContentType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.BasicDataPortletContentType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("BasicDataPortletPart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletPart")
                    .DisplayedAs(Consts.BasicDataPortletContentType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.WorkflowPortletContentType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("WorkflowPortletPart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletPart")
                    .DisplayedAs(Consts.BasicDataPortletContentType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.NavigationPortletContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("NavigationPortletPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletPart")
                .DisplayedAs(Consts.BasicDataPortletContentType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.IMAPTPortletContentType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IMapEmailPortletPart")
                    .WithPart("IdentityPart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletPart")
                    .DisplayedAs(Consts.IMAPTPortletContentType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.SidebarProjectionPortletType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("ProjectionWithDynamicSortPart")
                    .WithPart("IdentityPart")
                    .WithPart("TitlePart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletPart")
                    .DisplayedAs(Consts.SidebarProjectionPortletType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.SidebarProjectionPortletTemplateType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("ProjectionWithDynamicSortPart")
                .WithPart("IdentityPart")
                .WithPart("TitlePart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletTemplatePart")
                .DisplayedAs(Consts.SidebarProjectionPortletTemplateType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.SidebarStaticPortletTemplateType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("BodyPart")
                    .WithPart("IdentityPart")
                    .WithPart("TitlePart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletTemplatePart")
                    .DisplayedAs(Consts.SidebarStaticPortletTemplateType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.SidebarStaticPortletType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("BodyPart")
                .WithPart("IdentityPart")
                .WithPart("TitlePart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletPart")
                .DisplayedAs(Consts.SidebarStaticPortletType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.GenericDashboardContentType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("GenericDashboardPart")
                    .WithPart("TitlePart")
                    .WithPart("MenuPart")
                    .WithPart("ContainerPart")
                    .DisplayedAs(Consts.GenericDashboardContentType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.SidebarDashboardType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("SidebarDashboardPart")
                    .WithPart("TitlePart")
                    .WithPart("ContainerPart")
                    .DisplayedAs(Consts.SidebarDashboardType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.GenericCoverWidgetContentType,
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("CoverWidgetPart")
                    .WithPart("TitlePart")
                    .WithSetting("Stereotype", "Widget")
                    .DisplayedAs(Consts.CoverWidgetDisplay)
                );

            ContentDefinitionManager.AlterTypeDefinition(Consts.SidebarWidgetType,
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("CoverWidgetPart")
                    .WithSetting("Stereotype", "Widget")
                    .DisplayedAs("Sidebar")
                );

            GenerateStaticTemplates();

            GenericDashboardPortletDataTypes();
            return 3;
        }

        public int UpdateFrom1()
        {
            ContentDefinitionManager.AlterPartDefinition("DashboardManagementPart", builder => builder.Attachable());

            if (contentTypeRecordRepository.Value.Table.FirstOrDefault(c => c.Name == Consts.GenericCoverWidgetContentType) == null)
            {
                contentTypeRecordRepository.Value.Create(new ContentTypeRecord { Name = Consts.GenericCoverWidgetContentType });
                contentTypeRecordRepository.Value.Flush();
            }

            ContentDefinitionManager.AlterTypeDefinition(Consts.SidebarProjectionPortletTemplateType,
                cfg => cfg
                .RemovePart("DashboardPortletPart")
                .WithPart("DashboardPortletTemplatePart")
                .WithSetting(Consts.TemplateInstanceType, Consts.SidebarProjectionPortletType));

            ContentDefinitionManager.AlterTypeDefinition(Consts.SidebarStaticPortletTemplateType,
                cfg => cfg
                .RemovePart("DashboardPortletPart")
                .WithPart("DashboardPortletTemplatePart")
                .WithSetting(Consts.TemplateInstanceType, Consts.SidebarStaticPortletType));

            ContentDefinitionManager.AlterTypeDefinition(Consts.GenericCoverWidgetContentType,
              cfg => cfg
                  .WithPart("WidgetPart")
                  .WithPart("CommonPart")
                  .WithPart("IdentityPart")
                  .WithPart("CoverWidgetPart")
                  .WithPart("TitlePart")
                  .WithSetting("Stereotype", "Widget")
                  .DisplayedAs(Consts.CoverWidgetDisplay)
              );

            ContentDefinitionManager.AlterTypeDefinition(Consts.SidebarWidgetType,
                cfg => cfg
                    .RemovePart("SidebarPart")
                    .WithPart("CoverWidgetPart"));

            ContentDefinitionManager.AlterPartDefinition("CoverWidgetPart", builder => builder.Attachable());

            GenerateStaticTemplates();

            UpdateDataFrom1();

            return 2;
        }
        
        public int UpdateFrom2()
        {
            GenericDashboardPortletDataTypes();
            return 3;
        }

        private void GenericDashboardPortletDataTypes()
        {
            ContentDefinitionManager.AlterTypeDefinition(Consts.GenericProjectionPortletType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("ProjectionWithDynamicSortPart")
                    .WithPart("IdentityPart")
                    .WithPart("TitlePart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletPart")
                    .DisplayedAs(Consts.GenericProjectionPortletType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.GenericProjectionPortletTemplateType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("ProjectionWithDynamicSortPart")
                .WithPart("IdentityPart")
                .WithPart("TitlePart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletTemplatePart")
                .DisplayedAs(Consts.GenericProjectionPortletTemplateType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.GenericReportViewerPortletType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("DataReportViewerPart")
                    .WithPart("IdentityPart")
                    .WithPart("TitlePart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletPart")
                    .DisplayedAs(Consts.GenericReportViewerPortletType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.GenericReportViewerPortletTemplateType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("DataReportViewerPart")
                .WithPart("IdentityPart")
                .WithPart("TitlePart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletTemplatePart")
                .DisplayedAs(Consts.GenericReportViewerPortletTemplateType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.GenericActivityStreamPortletType,
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("ActivityStreamPart", c =>
                        c.WithSetting("ActivityStreamPartSettings.PageSize", "5"))
                    .WithPart("IdentityPart")
                    .WithPart("TitlePart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletPart")
                    .DisplayedAs(Consts.GenericActivityStreamPortletType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.GenericActivityStreamPortletTemplateType,
                cfg => cfg
                .WithPart("CommonPart")
                    .WithPart("ActivityStreamPart", c =>
                        c.WithSetting("ActivityStreamPartSettings.PageSize", "5"))
                .WithPart("IdentityPart")
                .WithPart("TitlePart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletTemplatePart")
                .DisplayedAs(Consts.GenericActivityStreamPortletTemplateType)
                .Creatable(false).Listable(false));
        }

        private void GenerateStaticTemplates()
        {
            ContentDefinitionManager.AlterTypeDefinition(Consts.DashboardManagementContentType + "Template",
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("DashboardManagementPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletTemplatePart")
                .DisplayedAs(Consts.DashboardManagementContentType + " Template")
                 .WithSetting(Consts.TemplateInstanceType, Consts.DashboardManagementContentType)
               .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.DashboardManagementContentType,
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("DashboardManagementPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletPart")
                .DisplayedAs(Consts.DashboardManagementContentType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.ContentManagementPortletContentType + "Template",
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("ContentManagementPortletPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletTemplatePart")
                .DisplayedAs(Consts.ContentManagementPortletContentType + " Template")
                .WithSetting(Consts.TemplateInstanceType, Consts.ContentManagementPortletContentType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.QueriesPortletContentType + "Template",
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("QueriesPortletPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletTemplatePart")
                .DisplayedAs(Consts.QueriesPortletContentType + " Template")
                .WithSetting(Consts.TemplateInstanceType, Consts.QueriesPortletContentType)
               .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.SmtpPortletContentType + "Template",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("SmtpEmailPortletPart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletTemplatePart")
                    .DisplayedAs(Consts.SmtpPortletContentType + " Template")
                    .WithSetting(Consts.TemplateInstanceType, Consts.SmtpPortletContentType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.BasicDataPortletContentType + "Template",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("BasicDataPortletPart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletTemplatePart")
                    .DisplayedAs(Consts.BasicDataPortletContentType + " Template")
                    .WithSetting(Consts.TemplateInstanceType, Consts.BasicDataPortletContentType)
                    .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.WorkflowPortletContentType + "Template",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("WorkflowPortletPart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletTemplatePart")
                    .DisplayedAs(Consts.WorkflowPortletContentType + " Template")
                  .WithSetting(Consts.TemplateInstanceType, Consts.WorkflowPortletContentType)
                  .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.NavigationPortletContentType + "Template",
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("NavigationPortletPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletTemplatePart")
                .DisplayedAs(Consts.NavigationPortletContentType + " Template")
                .WithSetting(Consts.TemplateInstanceType, Consts.NavigationPortletContentType)
                .Creatable(false).Listable(false));

            ContentDefinitionManager.AlterTypeDefinition(Consts.IMAPTPortletContentType + "Template",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IMapEmailPortletPart")
                    .WithPart("IdentityPart")
                    .WithPart("ContainablePart")
                    .WithPart("DashboardPortletTemplatePart")
                    .DisplayedAs(Consts.IMAPTPortletContentType + " Template")
                    .WithSetting(Consts.TemplateInstanceType, Consts.IMAPTPortletContentType)
                    .Creatable(false).Listable(false));


        }
        private void UpdateDataFrom1()
        {
            // get sidebars
            var sidebarWidgets = contentItemRecordRepository.Value.Table.Where(c => c.ContentType.Name == "SidebarWidget").ToList();

            foreach (var sidebar in sidebarWidgets)
            {
                sidebar.Data = sidebar.Data.Replace("SidebarPart", "CoverWidgetPart");
            }

            contentItemRecordRepository.Value.Flush();

            var sidebarDashboards = contentItemRecordRepository.Value.Table.Where(c => c.ContentType.Name == Consts.SidebarDashboardType).ToList();
            foreach (var sidebarDashboard in sidebarDashboards)
            {
                var data = XElement.Parse(sidebarDashboard.Data);
                var node = data.Descendants("SidebarDashboardPart").FirstOrDefault();
                var portletList = node.Attr("SidebarPortletList");

                // Add Template postfix to the portlet names
                portletList = string.Join(",", portletList.Split(',').Select(c => c.Trim() + "Template"));
                sidebarDashboard.Data = data.ToString();
            }

            var genericCoverWidget = this.contentTypeRecordRepository.Value.Table.FirstOrDefault(c => c.Name == Consts.GenericCoverWidgetContentType);

            var genericDashboardWidgets = contentItemRecordRepository.Value.Table.Where(c => c.ContentType.Name == "GenericDashboardWidget").ToList();
            foreach (var widget in genericDashboardWidgets)
            {
                var widgetContentItem = contentManager.Value.Get(widget.Id);
                widget.ContentType = new ContentTypeRecord { Id = genericCoverWidget.Id, Name = genericCoverWidget.Name };
                widget.Data = widget.Data.Replace("GenericDashboardPart", "CoverWidgetPart");

                var data = XElement.Parse(widget.Data);
                var coverWidgetNode = data.Descendants("CoverWidgetPart").FirstOrDefault();

                var newDashboard = this.contentManager.Value.Create(Consts.GenericDashboardContentType, VersionOptions.Draft);
                GenericDashboardPart genericDashboardPart = newDashboard.As<GenericDashboardPart>();

                // Add Template postfix to the portlet names
                var portletList = coverWidgetNode.Attr("PortletList");
                portletList = string.Join(",", portletList.Split(',').Select(c => c.Trim() + "Template"));

                genericDashboardPart.PortletList = portletList;
                genericDashboardPart.CreatePortletsOnPublishing = true;
                genericDashboardPart.ShowCollapsedInInitializedState = true;
                var commonPart = newDashboard.As<CommonPart>();
                commonPart.Owner = widgetContentItem.As<CommonPart>().Owner;
                contentManager.Value.Publish(newDashboard);

                coverWidgetNode.Attr("TargetContentItemId", newDashboard.Id.ToString(CultureInfo.InvariantCulture));
                coverWidgetNode.Attr("ContentType", newDashboard.ContentType);
                coverWidgetNode.Attr("LoadSync", "true");
                widget.Data = data.ToString();
            }

            contentItemRecordRepository.Value.Flush();

        }
    }
}