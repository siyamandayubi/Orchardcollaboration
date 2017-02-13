
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Database
{
    /// <summary>
    /// Contains common properties across SuiteCRM entities
    /// </summary>
    public interface IEntity
    {
        string id { get; set; }
        DateTime? date_modified { get; set; }
        DateTime? date_entered { get; set; }
        sbyte? deleted { get; set; }
    }
}
