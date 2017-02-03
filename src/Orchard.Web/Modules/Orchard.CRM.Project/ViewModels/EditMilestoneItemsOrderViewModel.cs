using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class EditMilestoneItemsOrderViewModel
    {
        private List<MilestoneItem> items = new List<MilestoneItem>();

        public List<MilestoneItem> Items
        {
            get
            {
                return this.items;
            }
        }

        public int ProjectId { get; set; }

        public class MilestoneItem
        {
            public int ContentItemId { get; set; }
            public int OrderId { get; set; }
            public int MilestoneId { get; set; }
        }
    }
}