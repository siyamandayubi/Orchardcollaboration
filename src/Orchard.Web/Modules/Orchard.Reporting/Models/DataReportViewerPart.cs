using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Models
{
    /*
     * select distinct civ.Id as Id
from Orchard.ContentManagement.Records.ContentItemVersionRecord as civ
join civ.ContentItemRecord as ci
join ci.FieldIndexPartRecord as fieldIndexPartRecord
join fieldIndexPartRecord.StringFieldIndexRecords as PageCategory
where (PageCategory.PropertyName = 'Page.Category.' and PageCategory.Value = 'Sport' ) AND (civ.Published = True)
order by civ.Id
     */
    public class DataReportViewerPart : ContentPart<DataReportViewerPartRecord>
    {
    }
}