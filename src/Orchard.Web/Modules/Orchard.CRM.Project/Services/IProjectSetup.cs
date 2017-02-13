using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.CRM.Project.Services
{
    public interface IProjectSetup : IDependency
    {
        void Setup3();
        void Setup2();
        void Setup();
    }
}
