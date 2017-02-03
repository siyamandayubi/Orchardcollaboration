using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Project.ViewModels
{
    public class UserViewModel
    {
        private List<SelectListItem> userProjects = new List<SelectListItem>();

        public List<SelectListItem> UserProjects { get { return this.userProjects; } }

        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string SkypeId { get; set; }
        public string Mobile { get; set; }
        public string Tel { get; set; }
        public string Fullname { get; set; }
        public string Thumbnail { get; set; }
        public string Tags { get; set; }
    }
}