using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Data;
using Orchard.Reporting.Models;
using Orchard.Reporting.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Reporting.Settings
{
    public class DataReportViewerSettingsEvents : ContentDefinitionEditorEventsBase
    {
        private IRepository<ReportRecord> reportRepository;

        public DataReportViewerSettingsEvents(IRepository<ReportRecord> reportRepository)
        {
            this.reportRepository = reportRepository;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition)
        {
            if (definition.PartDefinition.Name != "DataReportViewerPart")
                yield break;

            var settings = definition.Settings.GetModel<DataReportViewerPartSettings>();
            var model = new EditDataReportViewerViewModel();

            model.ChartTagCssClass = settings.ChartTagCssClass;
            model.ContainerCssClass = settings.ContainerTagCssClass;

            this.AddReportsToModel(model);
            yield return DefinitionTemplate(model, "DataReportViewerPartSettings", "DataReportViewerPartSettings");
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            if (builder.Name != "DataReportViewerPart")
                yield break;

            var settings = new DataReportViewerPartSettings();
            var model = new EditDataReportViewerViewModel();
            if (updateModel.TryUpdateModel(model, "DataReportViewerPartSettings", null, null))
            {
                settings.DefaultReportId = model.ReportId;
                settings.ContainerTagCssClass = model.ContainerCssClass;
                settings.ChartTagCssClass = model.ChartTagCssClass;
                settings.Build(builder);
            }

            this.AddReportsToModel(model);
            yield return DefinitionTemplate(model, "DataReportViewerPartSettings", "DataReportViewerPartSettings");
        }

        private void AddReportsToModel(EditDataReportViewerViewModel model)
        {
            var reports = this.reportRepository.Table.ToList();
            model.Reports.AddRange(reports.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Title,
                Selected = model.ReportId.HasValue ? model.ReportId.Value == c.Id : false
            }));
        }
    }
}