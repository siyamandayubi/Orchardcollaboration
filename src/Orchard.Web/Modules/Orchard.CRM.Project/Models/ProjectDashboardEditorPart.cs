/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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