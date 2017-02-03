using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.SuiteCRM.Connector.Models;

namespace Orchard.SuiteCRM.Connector
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("SuiteCRMSettingPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("SuiteCRMProjectPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("SuiteCRMTaskPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("SuiteCRMNotePart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("SuiteCRMUserPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("SuiteCRMStatusNotificationPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(SuiteCRMSettingPart.ContentItemTypeName,
                 cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("SuiteCRMSettingPart")
                 .DisplayedAs("SuiteCRM Settings"));

            ContentDefinitionManager.AlterTypeDefinition("ProjectItem",
                    cfg => cfg
                       .WithPart("SuiteCRMProjectPart"));

            ContentDefinitionManager.AlterTypeDefinition("Ticket",
                   cfg => cfg
                      .WithPart("SuiteCRMTaskPart"));

            ContentDefinitionManager.AlterTypeDefinition("CRMComment",
                 cfg => cfg
                    .WithPart("SuiteCRMNotePart"));

            ContentDefinitionManager.AlterTypeDefinition("User", c => c.WithPart("SuiteCRMUserPart"));

            ContentDefinitionManager.AlterTypeDefinition("SugarCRMSettingStatusPortlet",
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("SuiteCRMStatusNotificationPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletPart")
                .DisplayedAs("SugarCRM Setting Status Portlet")
                .Creatable(false).Listable(false));

            return 2;
        }

        public int UpdateFrom1()
        {
            ContentDefinitionManager.AlterTypeDefinition("SugarCRMSettingStatusPortlet",
                cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("BasicDataPortletPart")
                .WithPart("ContainablePart")
                .WithPart("DashboardPortletPart")
                .DisplayedAs("SugarCRM Setting Status Portlet")
                .Creatable(false).Listable(false));

            return 2;
        }
    }
}