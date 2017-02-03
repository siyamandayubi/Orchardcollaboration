using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.ViewModels
{
    public class EmailTemplateViewModel
    {
        private Collection<SelectListItem> types = new Collection<SelectListItem>();

        public int EmailTemplateId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        [Required]
        public string Subject { get; set; }

        [MaxLength(2000)]
        [Required]
        public string Text { get; set; }

        [Required]
        public int TypeId { get; set; }
        public string TypeName { get; set; }

        public Collection<SelectListItem> Types
        {
            get
            {
                return this.types;
            }
        }
    }
}