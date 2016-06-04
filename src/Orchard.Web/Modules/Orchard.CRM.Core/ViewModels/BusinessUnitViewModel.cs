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

namespace Orchard.CRM.Core.ViewModels
{
    using Orchard.Users.Models;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;

    public class BusinessUnitViewModel
    {
        private Collection<BusinessUnitViewModel> businessUnits = new Collection<BusinessUnitViewModel>();
        private Collection<TeamViewModel> teams = new Collection<TeamViewModel>();
        private Collection<UserPart> users = new Collection<UserPart>();

        public Collection<UserPart> Users
        {
            get
            {
                return this.users;
            }
        }

        public Collection<BusinessUnitViewModel> BusinessUnits
        {
            get
            {
                return this.businessUnits;
            }
        }

        public Collection<TeamViewModel> Teams
        {
            get
            {
                return this.teams;
            }
        }

        public int BusinessUnitId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }
        public bool Checked { get; set; }
    }
}