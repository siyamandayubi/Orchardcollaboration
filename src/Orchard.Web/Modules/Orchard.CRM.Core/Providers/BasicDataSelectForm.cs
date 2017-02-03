using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.CRM.Core.Providers
{
    public class BasicDataSelectForm : IDependency
    {
        public Localizer T { get; set; }

        protected IBasicDataService basicDataService;
        protected ITagBuilderFactory tagBuilderFactory;
        protected ICRMContentOwnershipService crmContentOwnershipService;
        private readonly ITokenizer tokenizer;

        public BasicDataSelectForm(
            ICRMContentOwnershipService crmContentOwnershipService,
            ITagBuilderFactory tagBuilderFactory,
            IBasicDataService basicDataService,
            ITokenizer tokenizer)
        {
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.basicDataService = basicDataService;
            this.tokenizer = tokenizer;
            this.tagBuilderFactory = tagBuilderFactory;
            this.T = NullLocalizer.Instance;
        }

        [Shape]
        public void DueDateOptions(dynamic Shape, dynamic Display, TextWriter Output, UrlHelper Url, int contentItemId)
        {
            Action<dynamic, SelectListItem> setTooltip = (tag, item) =>
            {
                DateTime dateTime = DateTime.Parse(item.Value);
                tag.MergeAttribute("title", dateTime.ToString("yyyy MMMM dd HH:mm:ss"));
                tag.MergeAttribute("data-text", dateTime.ToLongDateString());
            };

            List<SelectListItem> items = new List<SelectListItem>();
            string format = "yyyy MM dd HH:mm:ss";
            items.Add(new SelectListItem { Value = DateTime.UtcNow.AddDays(1).ToString(format), Text = T("Tomorrow").ToString() });
            items.Add(new SelectListItem { Value = DateTime.UtcNow.AddDays(3).ToString(format), Text = T("Three days later").ToString() });
            items.Add(new SelectListItem { Value = DateTime.UtcNow.AddDays(7).ToString(format), Text = T("One week later").ToString() });
            items.Add(new SelectListItem { Value = DateTime.UtcNow.AddMonths(1).ToString(format), Text = T("One month later").ToString() });
            this.RenderDropDownMenuList(Shape, Display, Output, Url, items, "DueDate", "UpdateDueDate", contentItemId, setTooltip);
        }

        [Shape]
        public void TicketPrioritiesOptions(dynamic Shape, dynamic Display, TextWriter Output, UrlHelper Url, int contentItemId)
        {
            var items = this.basicDataService.GetPriorities().OrderBy(c => c.OrderId).ToList();
            this.RenderDropDownMenuList(Shape, Display, Output, Url, items, "PriorityId", "UpdatePriority", contentItemId);
        }

        [Shape]
        public void TicketTicketTypesOptions(dynamic Shape, dynamic Display, TextWriter Output, UrlHelper Url, int contentItemId)
        {
            var items = this.basicDataService.GetTicketTypes().OrderBy(c => c.OrderId).ToList();
            this.RenderDropDownMenuList(Shape, Display, Output, Url, items, "TypeId", "UpdateTypeId", contentItemId);
        }

        [Shape]
        public void TicketServicesOptions(dynamic Shape, dynamic Display, TextWriter Output, UrlHelper Url, int contentItemId)
        {
            var items = this.basicDataService.GetServices().OrderBy(c => c.Name).Select(c => c.Record).ToList();
            this.RenderDropDownMenuList(Shape, Display, Output, Url, items, "ServiceId", "UpdateServiceId", contentItemId);
        }

        [Shape]
        public void TicketStatusOptions(dynamic Shape, dynamic Display, TextWriter Output, UrlHelper Url, int contentItemId)
        {
            var items = this.basicDataService.GetStatusRecords().OrderBy(c => c.OrderId).ToList();
            var basicDataItems = items.Select(c => new BasicDataRecordViewModel { Id = c.Id, Name = c.Name });
            this.RenderDropDownMenuList(Shape, Display, Output, Url, basicDataItems, "StatusId", "UpdateStatusId", contentItemId);
        }

        [Shape]
        public void StatusSelect(dynamic Shape, dynamic Display, TextWriter Output, string Name, int Size, string SelectedId, bool Multiple)
        {
            SelectedId = this.tokenizer.Replace(SelectedId, new Dictionary<string, object>());
            int? selectedId = null;

            int temp;
            if (int.TryParse(SelectedId, out temp))
            {
                selectedId = temp;
            }

            var statusItems = this.basicDataService.GetStatusRecords().ToList();
            List<SelectListItem> items = statusItems.Select(c => new
            SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Name,
                Selected = (selectedId.HasValue && c.Id == selectedId.Value)
            }).ToList();
            Shape.Value = selectedId;
            items.Insert(0, new SelectListItem { });

            Orchard.Forms.Shapes.EditorShapes editorShapes = new Forms.Shapes.EditorShapes(tagBuilderFactory);
            editorShapes.SelectList(
               Output: Output,
                Display: Display,
                Shape: Shape,
                Items: items,
                DataTextField: string.Empty,
                DataValueField: string.Empty,
                Name: Name,
                Size: Size,
                Multiple: Multiple);
        }

        private void AddNullItem(dynamic Shape, TextWriter Output, UrlHelper Url, string updateFieldName, int contentItemId)
        {
            var li = this.tagBuilderFactory.Create(Shape, "li");
            var a = this.tagBuilderFactory.Create(Shape, "a");
            Output.WriteLine(li.ToString(TagRenderMode.StartTag));

            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues["area"] = "Orchard.CRM.Core";
            routeValues["Ids"] = contentItemId;
            routeValues[updateFieldName] = true;
            string href = Url.Action("QuickUpdate", "Ticket", routeValues);

            a.MergeAttribute("href", href);
            a.MergeAttribute("data-text", "-");
            a.MergeAttribute("title", "-");

            Output.WriteLine(a.ToString(TagRenderMode.StartTag));
            Output.WriteLine(T("-"));
            Output.WriteLine(a.ToString(TagRenderMode.EndTag));
            Output.WriteLine(li.ToString(TagRenderMode.EndTag));
        }

        private void RenderDropDownMenuList(dynamic Shape, dynamic Display, TextWriter Output, UrlHelper Url, IEnumerable<SelectListItem> items, string fieldName, string updateFieldName, int contentItemId, Action<dynamic, SelectListItem> customizeTag)
        {
            var ul = this.tagBuilderFactory.Create(Shape, "ul");

            Output.WriteLine(ul.ToString(TagRenderMode.StartTag));

            if (items.Count() > 0)
            {
                this.AddNullItem(Shape, Output, Url, updateFieldName, contentItemId);
            }

            foreach (var item in items)
            {
                var li = this.tagBuilderFactory.Create(Shape, "li");
                var a = this.tagBuilderFactory.Create(Shape, "a");

                Output.WriteLine(li.ToString(TagRenderMode.StartTag));

                RouteValueDictionary routeValues = new RouteValueDictionary();
                routeValues["area"] = "Orchard.CRM.Core";
                routeValues["Ids"] = contentItemId;
                routeValues[fieldName] = item.Value;
                routeValues[updateFieldName] = true;
                string href = Url.Action("QuickUpdate", "Ticket", routeValues);

                a.MergeAttribute("href", href);

                if (customizeTag != null)
                {
                    customizeTag(a, item);
                }

                Output.WriteLine(a.ToString(TagRenderMode.StartTag));

                Output.WriteLine(T(item.Text).ToString());
                Output.WriteLine(a.ToString(TagRenderMode.EndTag));
                Output.WriteLine(li.ToString(TagRenderMode.EndTag));
            }

            Output.WriteLine(ul.ToString(TagRenderMode.EndTag));
        }
        private void RenderDropDownMenuList(dynamic Shape, dynamic Display, TextWriter Output, UrlHelper Url, IEnumerable<IBasicDataRecord> items, string fieldName, string updateFieldName, int contentItemId)
        {
            this.RenderDropDownMenuList(
                Shape,
                Display,
                Output,
                Url,
                items.Select(c => new SelectListItem { Value = c.Id.ToString(CultureInfo.InvariantCulture), Text = c.Name }),
                fieldName,
                updateFieldName,
                contentItemId,
                null);
        }
    }
}