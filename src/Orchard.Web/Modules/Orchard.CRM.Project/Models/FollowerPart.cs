using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Models
{
    public class FollowerPart : ContentPart
    {
        /// <summary>
        /// Comma seperated list of the follower user Ids
        /// </summary>
        public string Followers
        {
            get
            {
                return this.Retrieve(x => x.Followers);
            }
            set
            {
                this.Store(x => x.Followers, value);
            }
        }
    }
}