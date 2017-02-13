using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Schema;
using System;
using System.Linq;
using System.Data;
using Orchard.Reporting.Models;
using Orchard.ContentManagement;

namespace Orchard.Reporting
{
    public class Migrations : DataMigrationImpl
    {
        private readonly IRepository<ReportRecord> reportRecordRepository;
        private readonly IContentManager contentManager;

        public Migrations(IContentManager contentManager, IRepository<ReportRecord> reportRecordRepository)
        {
            this.reportRecordRepository = reportRecordRepository;
            this.contentManager = contentManager;
        }

        public int Create()
        {
            // Create ReportRecord table
            SchemaBuilder.CreateTable("ReportRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Title", c => c.WithLength(100).NotNull())
                .Column<string>("Name", c => c.WithLength(100).NotNull())
                .Column<int>("Query_Id", c => c.NotNull())
                .Column<int>("ChartType", c => c.NotNull())
                .Column<int>("AggregateMethod", c => c.NotNull())
                .Column<string>("State", c => c.Unlimited())
                .Column<string>("GroupByCategory", c => c.WithLength(100).NotNull())
                .Column<string>("GroupByType", c => c.WithLength(100).NotNull())
                .Column<string>("Guid", c => c.WithLength(36).Nullable()));

            // Create DataReportViewerPartRecord table
            SchemaBuilder.CreateTable("DataReportViewerPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("Report_Id", c => c.Nullable())
                .Column<string>("ContainerTagCssClass", c => c.Nullable().WithLength(100))
                .Column<string>("ChartTagCssClass", c => c.Nullable().WithLength(100)));

            ContentDefinitionManager.AlterPartDefinition("DataReportViewerPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("DataReportViewer", cfg => cfg
              .WithPart("CommonPart")
              .WithPart("TitlePart")
              .WithPart("DataReportViewerPart")
              .Creatable()
              .DisplayedAs("Data Report Viewer").Listable(true));

            ContentDefinitionManager.AlterTypeDefinition("DataReportViewerWidget", cfg => cfg
              .WithPart("CommonPart")
              .WithPart("DataReportViewerPart")
              .WithPart("WidgetPart")
              .WithSetting("Stereotype", "Widget")
              .DisplayedAs("Data Report Viewer Widget"));

            AddReportPartAndType();

            return 4;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTable("ReportRecord", table => table
            .AddColumn<string>("Guid", c => c.WithLength(36).Nullable()));

            AddReportPartAndType();

            // update current records
            var reports = reportRecordRepository.Table.ToList();
            reports.ForEach(c =>
            {
                c.Guid = Guid.NewGuid().ToString();
                var aggregateReportContent = contentManager.Create(ContentTypes.AggregateReportType);
                aggregateReportContent.As<ReportPart>().ReportId = c.Id;
                contentManager.Publish(aggregateReportContent);
            });

            reportRecordRepository.Flush();

            return 3;
        }

        public int UpdateFrom3()
        {
            ContentDefinitionManager.AlterTypeDefinition("DataReportViewer", cfg => cfg
              .WithPart("TitlePart").Listable(true));

            return 4;
        }

        private void AddReportPartAndType()
        {
            ContentDefinitionManager.AlterPartDefinition(ContentTypes.AggregateReportType, builder => builder.Attachable(false));

            ContentDefinitionManager.AlterTypeDefinition("DataReportViewer", cfg => cfg
                .WithPart("IdentityPart"));

            ContentDefinitionManager.AlterTypeDefinition("DataReportViewerWidget", cfg => cfg
                .WithPart("IdentityPart"));

            ContentDefinitionManager.AlterTypeDefinition("AggregateReport", cfg => cfg
              .WithPart("CommonPart")
              .WithPart("IdentityPart")
              .WithPart("ReportPart")
              .DisplayedAs("Aggregate Report"));
        }
    }
}