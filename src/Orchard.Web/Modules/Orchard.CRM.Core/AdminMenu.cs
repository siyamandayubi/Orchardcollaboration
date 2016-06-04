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

using Orchard.Localization;
using Orchard.UI.Navigation;
namespace Orchard.CRM.Core
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }
       
        public AdminMenu()
        {
            this.T = NullLocalizer.Instance;
        }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("Orchard Collaboration")
              .Add(T("Orchard Collaboration"),
                    menu => menu.Add(T("CRM Basic Data"), "4.0", item => item.Action("Index", "BasicData", new { area = "Orchard.CRM.Core" })
                    .LinkToFirstChild(true)
                    .Permission(Permissions.BasicDataPermission)))
                .Add(T("Orchard Collaboration"),
                    menu => menu.Add(T("Priorities"), "5.0", item => item.Action("Priorities", "BasicData", new { area = "Orchard.CRM.Core" })
                    .LinkToFirstChild(true)
                    .Permission(Permissions.BasicDataPermission)))
                .Add(T("Orchard Collaboration"),
                    menu => menu.Add(T("Email Templates"), "6.0", item => item.Action("Index", "EmailTemplate", new { area = "Orchard.CRM.Core" })
                    .LinkToFirstChild(true)
                    .Permission(Permissions.BasicDataPermission)))
                .Add(T("Orchard Collaboration"),
                    menu => menu.Add(T("Ticket Types"), "6.0", item => item.Action("TicketTypes", "BasicData", new { area = "Orchard.CRM.Core" })
                    .LinkToFirstChild(true)
                    .Permission(Permissions.BasicDataPermission)))
               .Add(T("Orchard Collaboration"),
                    menu => menu.Add(T("Ticket Status"), "7.0", item => item.Action("TicketStatusList", "BasicData", new { area = "Orchard.CRM.Core" })
                    .LinkToFirstChild(true)
                    .Permission(Permissions.BasicDataPermission)))
                .Add(T("Orchard Collaboration"),
                    menu => menu.Add(T("Services"), "8.0", item => item.Action("Services", "BasicData", new { area = "Orchard.CRM.Core" })
                    .LinkToFirstChild(true)
                    .Permission(Permissions.BasicDataPermission)))
                .Add(T("Orchard Collaboration"),
                    menu => menu.Add(T("Business Units"), "9.0", item => item.Action("BusinessUnits", "Organization", new { area = "Orchard.CRM.Core" })
                    .LinkToFirstChild(true)
                    .Permission(Permissions.BasicDataPermission)));
        }
    }
}