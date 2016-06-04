/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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

              ContentDefinitionManager.AlterTypeDefinition("TicketsDashboardWidget", c => c.WithPart("SuiteCRMStatusNotificationPart"));

              ContentDefinitionManager.AlterTypeDefinition("User", c => c.WithPart("SuiteCRMUserPart"));
            return 1;
        }
    }
}