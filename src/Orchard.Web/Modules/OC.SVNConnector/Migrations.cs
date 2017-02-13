using OC.SVNConnector.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System;

namespace OC.SVNConnector
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            // Create ServiceRecord table
            SchemaBuilder.CreateTable("SVNServerRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Server", c => c.WithLength(500))
                .Column<long>("LastRevision", c => c.WithDefault(0))
                .Column<DateTime>("FromDate", c => c.NotNull())
                );

            ContentDefinitionManager.AlterPartDefinition("SVNSettingsPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("SVNLogPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(SVNLogPart.ContentItemTypeName,
                 cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("SVNLogPart")
                 .DisplayedAs("SVN Log"));

            return 1;
        }
    }
}