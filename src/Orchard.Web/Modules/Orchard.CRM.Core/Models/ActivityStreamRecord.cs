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

namespace Orchard.CRM.Core.Models
{
    using Orchard.ContentManagement.Records;
    using Orchard.Users.Models;
    using System;

    public class ActivityStreamRecord
    {
        public virtual int Id { get; set; }
        public virtual ContentItemRecord RelatedContent { get; set; }
        public virtual ContentItemVersionRecord RelatedVersion { get; set; }
        public virtual string Description { get; set; }
        public virtual UserPartRecord User { get; set; }
        public DateTime CreationDateTime { get; set; }
    }
}