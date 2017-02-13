using Orchard.UI.Navigation;
using System;
namespace Orchard.CRM.Core.Services
{
    public interface IHelperService: IDependency
    {
        Pager ReterivePagerFromQueryString();
    }
}
