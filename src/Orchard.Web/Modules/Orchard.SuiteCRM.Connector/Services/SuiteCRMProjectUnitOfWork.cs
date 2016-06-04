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
    public class SuiteCRMProjectUnitOfWork : SuiteCRMUnitOfWork<project>
    {
        public SuiteCRMProjectUnitOfWork(DbConnection connection) : base(connection) { }
        public SuiteCRMProjectUnitOfWork(ISuiteCRMUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<project> GetProjects(int pageNumber, int pageSize)
        {
            if (pageSize == 0)
            {
                pageSize = 20;
            }

            if (pageNumber < 0)
            {
                pageNumber = 0;
            }

            return this.context
                .projects
                .Where(c => c.deleted == 0)
                .OrderByDescending(c=>c.date_entered)
                .Skip(pageSize * pageNumber)
                .Take(pageSize)
                .ToList();
        }

        public int Count()
        {
            return this.context.projects.Where(c => c.deleted == 0).Count();
        }

        public IEnumerable<project> GetProjects(string[] ids)
        {
            return this.context.projects.Where(c => c.deleted == 0 && ids.Contains(c.id)).ToList();
        }
    }
}
