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