using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.ViewModels
{
    public class EditAttachToViewModel
    {
        private Collection<SelectListItem> items = new Collection<SelectListItem>();

        public Collection<SelectListItem> Items
        {
            get
            {
                return this.items;
            }
        }

        public int? ParentId { get; set; }
        public string ParentName { get; set; }

        public int? SelectedId { get; set; }

        public int? Size { get; set; }
    }
}