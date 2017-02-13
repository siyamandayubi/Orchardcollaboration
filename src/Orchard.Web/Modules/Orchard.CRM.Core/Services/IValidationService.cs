namespace Orchard.CRM.Core.Services
{
    using Orchard.CRM.Core.Models;
    using System.Web.Mvc;

    public interface IValidationService : IDependency
    {
        bool IsStatusTypeChangeValid(StatusRecord changedRecord, ModelStateDictionary modelState);
    }
}