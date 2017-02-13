using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public interface ICRMSetup : IDependency
    {
        void AddBasicData();
    }
}