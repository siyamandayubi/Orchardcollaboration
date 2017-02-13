using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class InviteUserToProjectPostViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int[] Projects { get; set; }
    }
}