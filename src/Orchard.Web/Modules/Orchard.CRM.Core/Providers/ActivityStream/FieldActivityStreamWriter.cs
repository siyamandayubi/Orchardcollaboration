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

namespace Orchard.CRM.Core.Providers.ActivityStream
{
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Services;
    using Orchard.Localization;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;

    public class FieldActivityStreamWriter : IActivityStreamWriter
    {
        public FieldActivityStreamWriter()
        {
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            return true;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                yield break;
            }

            var oldValues = context.Snapshot as IDictionary<string, object>;

            foreach (var part in context.ContentItem.Parts)
            {
                var oldPartValues = oldValues != null && oldValues.ContainsKey(part.PartDefinition.Name) ? oldValues[part.PartDefinition.Name] as IDictionary<string, object> : null;
                var oldPartFields = oldPartValues != null && oldPartValues.ContainsKey("Fields") ? oldPartValues["Fields"] as IDictionary<string, object> : null;

                foreach (var field in part.Fields)
                {
                    var oldValue = oldPartFields != null && oldPartFields.ContainsKey(field.Name) ? oldPartFields[field.Name] : string.Empty;
                    var newValue = CRMHelper.ReteriveField(part, field.Name);

                    if (oldValue == null && newValue != null)
                    {
                        string change = string.Format(CultureInfo.CurrentUICulture,
                        "{0} is set to {1}",
                        T(field.Name).Text,
                        newValue ?? T("[NULL]").Text);
                        yield return new ActivityStreamChangeItem(change);
                    }
                    else if (newValue == null && oldValue != null)
                    {
                        string change = string.Format(CultureInfo.CurrentUICulture,
                        "{0} is changed to NULL",
                        T(field.Name).Text);
                        yield return new ActivityStreamChangeItem(change);
                    }
                    else if (oldValue != null && newValue != null && oldValue.ToString() != newValue.ToString())
                    {
                        string change = string.Format(CultureInfo.CurrentUICulture,
                        "{0} is changed to '{1}'",
                        T(field.Name).Text,
                        newValue ?? T("[NULL]").Text);
                        yield return new ActivityStreamChangeItem(change);
                    }
                }
            }
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            return null;
        }

        public string Name
        {
            get { return "Fields"; }
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            dynamic oldData = new ExpandoObject();
            var oldDataDictionary = oldData as IDictionary<string, object>;
            foreach (var part in contentItem.Parts)
            {
                dynamic partOldData = new ExpandoObject();
                oldDataDictionary.Add(part.PartDefinition.Name, partOldData);

                var fields = new Dictionary<string, object>();
                partOldData.Fields = fields;

                foreach (var field in part.Fields)
                {
                    fields[field.Name] = CRMHelper.ReteriveField(part, field.Name);
                }
            }

            return oldData;
        }
    }
}