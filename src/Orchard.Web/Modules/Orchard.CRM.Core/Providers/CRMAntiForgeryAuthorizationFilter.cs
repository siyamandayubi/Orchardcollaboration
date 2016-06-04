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

using System;
using System.Collections.Specialized;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Mvc.AntiForgery;
using Orchard.Mvc;
using System.Globalization;

namespace Orchard.CRM.Core.Providers
{
    public class CRMAntiForgeryAuthorizationFilter : FilterProvider, IAuthorizationFilter
    {
        private readonly ISiteService _siteService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IExtensionManager _extensionManager;

        private const string FieldName = "__RequestVerificationToken";

        public CRMAntiForgeryAuthorizationFilter(ISiteService siteService, IAuthenticationService authenticationService, IExtensionManager extensionManager)
        {
            _siteService = siteService;
            _authenticationService = authenticationService;
            _extensionManager = extensionManager;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {

            if ((filterContext.HttpContext.Request.HttpMethod != "POST" ||
                 _authenticationService.GetAuthenticatedUser() == null) && !ShouldValidateGet(filterContext))
            {
                return;
            }

            if (!IsAntiForgeryProtectionEnabled(filterContext))
            {
                return;
            }

            bool fileUploaderRequest = filterContext.RouteData.Values.ContainsKey("controller");

            if (fileUploaderRequest)
            {
                var controller = filterContext.RouteData.Values["controller"];
                fileUploaderRequest = controller.ToString().ToLower(CultureInfo.InvariantCulture) == "fileupload";
            }

            if (!fileUploaderRequest)
            {
                var validator = new ValidateAntiForgeryTokenAttribute();
                validator.OnAuthorization(filterContext);
            }
            else
            {
                string key =  FieldName;
                for(var i = 0; i < filterContext.HttpContext.Request.Cookies.Count; i ++){
                    var temp = filterContext.HttpContext.Request.Cookies.GetKey(i);
                    if(temp.StartsWith(FieldName)){
                        key = temp;
                        break;
                    }
                }

                var httpCookie = filterContext.HttpContext.Request.Cookies[key];
                var value = filterContext.RequestContext.HttpContext.Request.QueryString[FieldName];

                if (string.IsNullOrEmpty(value))
                {
                    value = filterContext.RequestContext.HttpContext.Request.Form[FieldName];
                }

                if(string.IsNullOrEmpty(value) || httpCookie == null)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    System.Web.Helpers.AntiForgery.Validate(httpCookie.Value, value);
                }
            }

            if (filterContext.HttpContext is HackHttpContext)
                filterContext.HttpContext = ((HackHttpContext)filterContext.HttpContext).OriginalHttpContextBase;
        }

        private bool IsAntiForgeryProtectionEnabled(ControllerContext context)
        {
            string currentModule = GetArea(context.RouteData);

            return currentModule.ToLower(CultureInfo.InvariantCulture).CompareTo("orchard.crm.core") == 0;
        }

        private static string GetArea(RouteData routeData)
        {
            if (routeData.Values.ContainsKey("area"))
                return routeData.Values["area"] as string;

            return routeData.DataTokens["area"] as string ?? "";
        }

        private static bool ShouldValidateGet(AuthorizationContext context)
        {
            const string tokenFieldName = "__RequestVerificationToken";

            var attributes =
                (ValidateAntiForgeryTokenOrchardAttribute[])
                context.ActionDescriptor.GetCustomAttributes(typeof(ValidateAntiForgeryTokenOrchardAttribute), false);

            if (attributes.Length > 0)
            {
                var request = context.HttpContext.Request;

                //HAACK: (erikpo) If the token is in the querystring, put it in the form so MVC can validate it
                if (!string.IsNullOrEmpty(request.QueryString[tokenFieldName]))
                {
                    context.HttpContext = new HackHttpContext(context.HttpContext, (HttpContext)context.HttpContext.Items["originalHttpContext"]);
                    ((HackHttpRequest)context.HttpContext.Request).AddFormValue(tokenFieldName, context.HttpContext.Request.QueryString[tokenFieldName]);
                }

                return true;
            }

            return false;
        }

        #region HackHttpContext

        private class HackHttpContext : HttpContextWrapper
        {
            private readonly HttpContextBase _originalHttpContextBase;
            private readonly HttpContext _originalHttpContext;
            private HttpRequestWrapper _request;

            public HackHttpContext(HttpContextBase httpContextBase, HttpContext httpContext)
                : base(httpContext)
            {
                _originalHttpContextBase = httpContextBase;
                _originalHttpContext = httpContext;
            }

            public HttpContextBase OriginalHttpContextBase
            {
                get { return _originalHttpContextBase; }
            }

            public override HttpRequestBase Request
            {
                get
                {
                    if (_request == null)
                        _request = new HackHttpRequest(_originalHttpContext.Request);

                    return _request;
                }
            }
        }

        #endregion

        #region HackHttpRequest

        private class HackHttpRequest : HttpRequestWrapper
        {
            private readonly HttpRequest _originalHttpRequest;
            private NameValueCollection _form;

            public HackHttpRequest(HttpRequest httpRequest)
                : base(httpRequest)
            {
                _originalHttpRequest = httpRequest;
            }

            public override NameValueCollection Form
            {
                get
                {
                    if (_form == null)
                        _form = new NameValueCollection(_originalHttpRequest.Form);

                    return _form;
                }
            }

            public void AddFormValue(string key, string value)
            {
                Form.Add(key, value);
            }
        }

        #endregion
    }
}