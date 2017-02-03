using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Models
{
    /// <summary>
    /// This ContentPart is responsible for keeping the record of added Portlets to the ProjectDashboard
    /// </summary>
    public class ProjectDashboardEditorPart : ContentPart
    {
        public int[] PortletList
        {
            get
            {
                // The array has been stored as a comma sperated list, because int[] is not supported by Reterive method
                string serializedPortIds = Retrieve<string>("PortletList");
                if (string.IsNullOrEmpty(serializedPortIds))
                {
                    return null;
                }

                return serializedPortIds.Split(',').Select(c => int.Parse(c)).ToArray();
            }
            set
            {
                // The array has been stored as a comma sperated list, because int[] is not supported by Reterive method
                string serializedPortIds = string.Empty;
                if (value != null)
                {
                    serializedPortIds = string.Join(",", value.Select(c => c.ToString(CultureInfo.InvariantCulture)));
                }

                Store("PortletList", serializedPortIds);
            }
        }
    }
}