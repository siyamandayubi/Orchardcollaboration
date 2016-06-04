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

using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMProjectTaskUnitOfWork : SuiteCRMUnitOfWork<project_task>
    {
        public SuiteCRMProjectTaskUnitOfWork(DbConnection connection) : base(connection) { }
        public SuiteCRMProjectTaskUnitOfWork(ISuiteCRMUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<project_task> GetProjectTasks(string projectId)
        {
            return this.context.project_tasks.Where(c => c.deleted == 0 && c.project_id == projectId).ToList();
        }

        public IEnumerable<project_task> GetTasks(string[] ids)
        {
            return this.context.project_tasks.Where(c => c.deleted == 0 && ids.Contains(c.id)).ToList();
        }
    }
}
