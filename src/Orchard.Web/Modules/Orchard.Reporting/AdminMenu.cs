using Orchard.Localization;
using Orchard.Projections;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("Data Reporting").Add(T("Data Reports"), "3",
                menu =>
                    menu.Add(T("Data Reports"), "1.0",
                    q => q.Action("Index", "Report", new { area = "Orchard.Reporting" }).Permission(Permissions.ManageQueries).LocalNav()), null);
        }
    }
}