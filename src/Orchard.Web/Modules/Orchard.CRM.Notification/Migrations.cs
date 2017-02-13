using Orchard.Data.Migration;
using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;

namespace Orchard.CRM.Notification
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            // Create UserContentVisitRecord table
            SchemaBuilder.CreateTable("UserContentVisitRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<int>("User_Id", c => c.NotNull())
                .Column<int>("LastVisitedActivityStreamId", c => c.NotNull())
                .Column<DateTime>("LastVisitTime", c => c.NotNull())
                );

            ContentDefinitionManager.AlterPartDefinition("CRMNotificationWidgetPart", c => c.Attachable());
            ContentDefinitionManager.AlterPartDefinition("NotificationStreamPart", c => c.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(Consts.CRMNotificationWidgetType,
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("CRMNotificationWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithSetting("Stereotype", "Widget")
                );

            ContentDefinitionManager.AlterTypeDefinition(Consts.CRMNotificationListType,
                cfg => cfg
                    .WithPart("NotificationStreamPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                ); 
            
            return 1;
        }
    }
}