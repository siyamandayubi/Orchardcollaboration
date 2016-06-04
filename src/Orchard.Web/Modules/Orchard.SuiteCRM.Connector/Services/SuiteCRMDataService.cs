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

using Orchard.Logging;
using Orchard.SuiteCRM.Connector.Database;
using Orchard.SuiteCRM.Connector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using MySql.Data.MySqlClient;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMDataService : ISuiteCRMDataService
    {
        private readonly IOrchardServices orchardServices;
        public ILogger Logger { get; set; }

        public SuiteCRMDataService(IOrchardServices orchardServices)
        {
            this.orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public IEnumerable<project> GetProjects(string[] ids)
        {
            List<project> projects = new List<project>();
            
            using (SuiteCRMProjectUnitOfWork repository = new SuiteCRMProjectUnitOfWork(Helper.GetConnection(this.orchardServices, this.Logger)))
            {
                projects = repository.GetProjects(ids).ToList();
            }

            return projects;
        }

        public email_addr_bean_rel GetContact(string email)
        {
            using (SuiteCRMEmailAddressBeanUnitOfWork repository = new SuiteCRMEmailAddressBeanUnitOfWork(Helper.GetConnection(this.orchardServices,this.Logger)))
            {
                return repository.GetByEmail(email);
            }
        }

        public IEnumerable<project> GetProjects(int pageNumber, int pageSize)
        {
            if (pageSize == 0)
            {
                pageSize = 20;
            }

            var setting = this.orchardServices.WorkContext.CurrentSite.As<SuiteCRMSettingPart>();

            List<project> projects = new List<project>();

            using (SuiteCRMProjectUnitOfWork repository = new SuiteCRMProjectUnitOfWork(Helper.GetConnection(this.orchardServices, this.Logger)))
            {
                projects = repository.GetProjects(pageNumber, pageSize).ToList();
            }

            return projects;
        }

        public int ProjectCount()
        {
            using (SuiteCRMProjectUnitOfWork repository = new SuiteCRMProjectUnitOfWork(Helper.GetConnection(this.orchardServices, this.Logger)))
            {
                return repository.Count();
            }
        }
    }
}