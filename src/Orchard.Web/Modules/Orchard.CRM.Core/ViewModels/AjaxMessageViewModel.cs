using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class AjaxMessageViewModel
    {
        private Collection<KeyValuePair<string, string>> errors = new Collection<KeyValuePair<string, string>>();

        public int Id { get; set; }

        public bool IsDone { get; set; }

        public Collection<KeyValuePair<string,string>> Errors
        {
            get
            {
                return this.errors;
            }
        }

        public string Html { get; set; }

        public object Data { get; set; }
    }
}