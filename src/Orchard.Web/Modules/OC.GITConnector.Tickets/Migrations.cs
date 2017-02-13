using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace OC.GITConnector.Tickets
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("GITCommitsTicketPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("Ticket", cfg => cfg.WithPart("GITCommitsTicketPart"));
            return 1;
        }
    }
}