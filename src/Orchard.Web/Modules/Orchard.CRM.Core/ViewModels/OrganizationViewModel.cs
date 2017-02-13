using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class OrganizationViewModel
    {
        private Collection<BusinessUnitViewModel> businessUnits = new Collection<BusinessUnitViewModel>();

        public Collection<BusinessUnitViewModel> BusinessUnits
        {
            get
            {
                return this.businessUnits;
            }
        }
    }
}