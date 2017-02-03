using Orchard.ContentPicker.Tokens;
using Orchard.CRM.Core.Services;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Orchard.CRM.Core.Providers
{
    public class UserTokenProvider : Orchard.Tokens.ITokenProvider
    {
        private readonly IOrchardServices services;

        public const string LoginedUser = "LoggedOnUser";
        private Localizer T { get; set; }

        public UserTokenProvider(IOrchardServices services)
        {
            this.services = services;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context)
        {
            context.For(UserTokenProvider.LoginedUser, T("EUser"), T("User"))
                 .Token("Id", T("Id"), T("Id"))
                 .Token("Email", T("Email"), T("Email"))
                 .Token("Username", T("Username"), T("Username"));
        }

        public void Evaluate(EvaluateContext context)
        {
            // User
            context.For(UserTokenProvider.LoginedUser, () => context.Data)
                .Token("Email", c =>
                {
                    var user = services.WorkContext.CurrentUser;
                    if (user == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return user.Email;
                    }
                })
                .Token("Id", c =>
                {
                    var user = services.WorkContext.CurrentUser;
                    if (user == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return user.Id;
                    }
                })
                .Token("Username", c =>
                {
                    var user = services.WorkContext.CurrentUser;
                    if (user == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return user.UserName;
                    }
                })
                .Token("FullName", contextParameter =>
                {
                    var user = services.WorkContext.CurrentUser;
                    if (user == null)
                    {
                        return string.Empty;
                    }

                    return CRMHelper.GetFullNameOfUser(user.As<UserPart>());
                });
        }
    }
}