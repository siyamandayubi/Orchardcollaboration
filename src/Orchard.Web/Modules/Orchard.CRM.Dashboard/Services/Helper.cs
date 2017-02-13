using Orchard.ContentManagement.MetaData;
using System.Linq;

namespace Orchard.CRM.Dashboard.Services
{
    public static class Helper
    {
        public static string[] GetGenericDashboardAvailablePortletTypes(IContentDefinitionManager contentDefinitionManager)
        {
            var sidebarSpecificTypes = new[] { Consts.SidebarProjectionPortletTemplateType, Consts.SidebarStaticPortletType };
            var types =
                contentDefinitionManager.ListTypeDefinitions()
                    .Where(c => c.Parts.Any(d => d.PartDefinition.Name == "DashboardPortletTemplatePart"))
                    .Where(c => !sidebarSpecificTypes.Contains(c.Name))
                    .ToList();

            return types.Select(c => c.Name).ToArray();
        }
    }
}