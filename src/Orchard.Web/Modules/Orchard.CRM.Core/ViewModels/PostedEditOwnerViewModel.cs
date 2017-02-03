using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class PostedEditOwnerViewModel
    {
        /// <summary>
        /// The GroupId format is "Team:{Id}" or "BusinessUnit:{Id}". That's the reason that the field type is string
        /// </summary>
        public string GroupId { get; set; }
        public int? UserId { get; set; }
    }
}