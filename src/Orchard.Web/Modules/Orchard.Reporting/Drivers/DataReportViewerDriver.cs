using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Services;
using Orchard.Reporting.Models;
using Orchard.Reporting.Services;
using Orchard.Reporting.Settings;
using Orchard.Reporting.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Orchard.Reporting.Drivers
{
    public class DataReportViewerDriver : ContentPartDriver<DataReportViewerPart>
    {
        private readonly IReportManager reportManger;
        private readonly IRepository<ReportRecord> reportRepository;
        private readonly IProjectionManager projectionManager;

        public DataReportViewerDriver(
            IReportManager reportManger,
            IProjectionManager projectionManager,
            IRepository<ReportRecord> reportRepository)
        {
            this.projectionManager = projectionManager;
            this.reportRepository = reportRepository;
            this.reportManger = reportManger;
            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override DriverResult Display(DataReportViewerPart part, string displayType, dynamic shapeHelper)
        {
            if (part.Record.Report == null)
            {
                return null;
            }

            if (displayType == "Detail")
            {
                var report = this.reportRepository.Table.FirstOrDefault(c => c.Id == part.Record.Report.Id);

                if (report == null)
                {
                    return null;
                }

                var reportData = this.reportManger.RunReport(report, part.ContentItem);
                int count = this.reportManger.GetCount(report, part.ContentItem);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var model = new DataReportViewerViewModel
                {
                    TotalCount = count,
                    ReportTitle = part.Record.Report.Title,
                    JsonData = serializer.Serialize(reportData.Select(c=> new {Label = c.Label, Value = c.AggregationValue}).ToArray()),
                    Data = reportData.ToList(),
                    ChartCssClass = part.Record.ChartTagCssClass,
                    ContainerCssClass = part.Record.ContainerTagCssClass
                };

                if (report.ChartType == (int)ChartTypes.PieChart)
                {
                    return ContentShape("Parts_DataReportViewer_PieChart",
                         () => shapeHelper.Parts_DataReportViewer_PieChart(
                             Model: model
                             ));
                }
                else if (report.ChartType == (int)ChartTypes.SimpleList)
                {
                    return ContentShape("Parts_DataReportViewer_SimpleList",
                         () => shapeHelper.Parts_DataReportViewer_SimpleList(
                             Model: model
                             ));
                }
                else
                {
                    return ContentShape("Parts_DataReportViewer",
                          () => shapeHelper.Parts_DataReportViewer_Summary(
                              Model: new DataReportViewerViewModel { ReportTitle = part.Record.Report.Title }
                              ));
                }
            }
            else
            {
                return ContentShape("Parts_DataReportViewer",
                     () => shapeHelper.Parts_DataReportViewer_Summary(
                         Model: new DataReportViewerViewModel { ReportTitle = part.Record.Report.Title}
                         ));
            }
        }

        protected override DriverResult Editor(DataReportViewerPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            EditDataReportViewerViewModel model = new EditDataReportViewerViewModel();
            updater.TryUpdateModel(model, "DataReportViewerPart", null, null);

            if (model.ReportId == null)
            {
                updater.AddModelError("ReportId", T("Please select a Report"));
            }
            else
            {
                part.Record.Report = new ReportRecord { Id = model.ReportId.Value };
                part.Record.ChartTagCssClass = model.ChartTagCssClass;
                part.Record.ContainerTagCssClass = model.ContainerCssClass;
            }

            return this.Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(DataReportViewerPart part, dynamic shapeHelper)
        {
            var model = new EditDataReportViewerViewModel();
            var record = part.Record;

            // if it is called for creating a new item, then the default values from settings must be copied into the part
            if (record.Id == default(int))
            {
                var settings = part.TypePartDefinition.Settings.GetModel<DataReportViewerPartSettings>();
                model.ChartTagCssClass = settings.ChartTagCssClass;
                model.ContainerCssClass = settings.ContainerTagCssClass;
                model.ReportId = settings.DefaultReportId;
            }
            else
            {
                if (record.Report != null)
                {
                    model.ReportId = record.Report.Id;
                }

                model.ChartTagCssClass = record.ChartTagCssClass;
                model.ContainerCssClass = record.ContainerTagCssClass;
            }

            var reports = this.reportRepository.Table.ToList();
            model.Reports.AddRange(reports.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Title,
                Selected = model.ReportId.HasValue ? model.ReportId.Value == c.Id : false
            }));

            return ContentShape("Parts_DataReportViewer_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/DataReportViewer",
                        Model: model,
                        Prefix: Prefix));
        }
    }
}