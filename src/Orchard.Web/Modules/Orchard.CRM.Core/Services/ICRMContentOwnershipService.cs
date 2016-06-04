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

using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Services
{
    public interface ICRMContentOwnershipService : IDependency
    {
        bool IsOperator(int userId);
        bool IsCustomer(int userId);
        bool CurrentUserCanChangePermission(IContent contentItem);
        bool CurrentUserCanChangePermission(IContent item, ModelStateDictionary modelState);
        bool CurrentUserIsContentItemAssignee(IContent item);
        bool CurrentUserCanEditContent(IContent item);
        bool CurrentUserCanDeletePermission(int permissionId, IContent item, ModelStateDictionary modelState);
        bool CurrentUserCanViewContent(IContent item);
        bool IsCurrentUserCustomer();
        bool IsCurrentUserOperator();
        bool IsCurrentUserAdvanceOperator();
    }
}