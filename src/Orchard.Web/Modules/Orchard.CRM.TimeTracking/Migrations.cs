using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.TimeTracking
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("TimeTrackingItemRecord", table => table.ContentPartRecord()
                .Column<int>("User_Id", c => c.Nullable())
                .Column<int>("TimeTrackingPartRecord_Id", c => c.Nullable())
                .Column<string>("OriginalTimeTrackingString", c => c.Nullable().WithLength(100))
                .Column<int>("TimeInMinute", c => c.Nullable())
                .Column<string>("Comment", c => c.Nullable().WithLength(500))
                .Column<DateTime>("TrackingDate", c => c.Nullable())
                );

            // Create TimeTrackingPartRecord table
            SchemaBuilder.CreateTable("TimeTrackingPartRecord", table => table
                .ContentPartRecord());

            // Parts
            ContentDefinitionManager.AlterPartDefinition("TimeTrackingItemPart", c => c.Attachable());
            ContentDefinitionManager.AlterPartDefinition("TimeTrackingPart", c => c.Attachable());

            // Add TimeTracking to ticket
            ContentDefinitionManager.AlterTypeDefinition("Ticket", cfg => cfg.WithPart("TimeTrackingPart"));

            ContentDefinitionManager.AlterTypeDefinition(ContentTypes.TimeTrackingItemType,
                cfg => cfg
                    .WithPart("TimeTrackingItemPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .DisplayedAs("Time Tracking Item")
                    .Creatable(false));

            return 1;
        }
    }
}