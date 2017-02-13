using OC.GITConnector.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System;

namespace OC.GITConnector
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            // Create ServiceRecord table
            SchemaBuilder.CreateTable("GITServerRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Server", c => c.WithLength(500))
                .Column<long>("LastRevision", c => c.WithDefault(0))
                .Column<DateTime>("FromDate", c => c.NotNull())
                );

            // Create GITServerBranchRecord table
            SchemaBuilder.CreateTable("GITServerBranchRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<int>("ServerRecord_Id", c => c.NotNull())
                .Column<string>("BranchName", c => c.WithLength(200).NotNull())
                .Column<string>("Sha", c => c.WithLength(500).Nullable())
                .Column<DateTime>("LastUpdate", c => c.NotNull())
                );

            ContentDefinitionManager.AlterPartDefinition("GITSettingsPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("GITCommitPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(GITCommitPart.ContentItemTypeName,
                 cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("GITCommitPart")
                 .DisplayedAs("GIT Log"));

            return 1;
        }
    }
}