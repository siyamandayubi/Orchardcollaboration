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


using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.ViewModels
{
    public class MainViewModel
    {
        private List<SuiteCRMProjectDetailViewModel> projects = new List<SuiteCRMProjectDetailViewModel>();
        private List<SuiteCRMUserViewModel> users = new List<SuiteCRMUserViewModel>();

        public List<SuiteCRMUserViewModel> Users { get { return this.users; } }
        public List<SuiteCRMProjectDetailViewModel> Projects { get { return this.projects; } }

        public bool ViewUsersPage { get; set; }
        public int SuiteCRMUsersCount { get; set; }
        public int PageSize { get; set; }
        public int SuiteCRMProjectsCount { get; set; }
        public int OrchardCollaborationProjectsCount { get; set; }
        public int SuiteCRMPage { get; set; }
        public int OrchardCollaborationPage { get;set; }
        public bool ListedBasedOnSuiteCRM { get; set; }

        public dynamic TranslateTable { get; set; }
        public dynamic Routes { get; set; }
    }
}