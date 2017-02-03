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