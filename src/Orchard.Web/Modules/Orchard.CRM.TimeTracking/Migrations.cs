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
            SchemaBuilder.CreateTable("TimeTrackingItemRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<int>("User_Id", c => c.NotNull())
                .Column<int>("TimeTrackingPartRecord_Id", c => c.NotNull())
                .Column<string>("OriginalTimeTrackingString", c => c.NotNull().WithLength(100))
                .Column<int>("TimeInMinute", c => c.NotNull())
                .Column<string>("Comment", c => c.NotNull().WithLength(500))
                .Column<DateTime>("TrackingDate", c => c.NotNull())
                );

            // Create TimeTrackingPartRecord table
            SchemaBuilder.CreateTable("TimeTrackingPartRecord", table => table
                .ContentPartRecord());

            // TimeTrackingPart
            ContentDefinitionManager.AlterPartDefinition("TimeTrackingPart", c => c.Attachable());

            // Add TimeTracking to ticket
            ContentDefinitionManager.AlterTypeDefinition("Ticket", cfg => cfg.WithPart("TimeTrackingPart"));

            return 1;
        }
    }
}