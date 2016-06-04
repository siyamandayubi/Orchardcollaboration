using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using S22.IMAP.Models;

namespace S22.IMAP
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            // Create ServiceRecord table
            SchemaBuilder.CreateTable("IMAPHostRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Host", c => c.WithLength(50))
                .Column<long>("EmailUid", c=>c.WithDefault(0))
                .Column<DateTime>("FromDate", c => c.NotNull())
                );

            ContentDefinitionManager.AlterPartDefinition("IMAPEmailPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(IMAPEmailPart.ContentItemTypeName,
                 cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IMAPEmailPart")
                 .DisplayedAs("IMAP Email"));
            return 1;
        }
    }
}