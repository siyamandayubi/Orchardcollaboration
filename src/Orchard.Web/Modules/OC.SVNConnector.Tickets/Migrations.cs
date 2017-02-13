using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace OC.SVNConnector.Tickets
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("SVNLogsTicketPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("Ticket", cfg => cfg.WithPart("SVNLogsTicketPart"));
            return 1;
        }
    }
}