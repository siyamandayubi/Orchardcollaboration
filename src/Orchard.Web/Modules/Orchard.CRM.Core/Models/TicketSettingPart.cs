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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class TicketSettingPart : ContentPart
    {
        public const int DefaultCustomerTicketDueDateDays = 4;
        public const string DefaultTicketsForContentItemsMenuFlipFlopPosition = "top-left";
        public int CustomerTicketDueDateDays
        {
            get
            {
                var value = this.Retrieve(x => x.CustomerTicketDueDateDays);

                if (value == default(int))
                {
                    value = DefaultCustomerTicketDueDateDays;
                }

                return value;
            }
            set
            {
                this.Store(x => x.CustomerTicketDueDateDays, value);
            }
        }

        public string TicketsForContentItemsMenuFlipFlopPosition
        {
            get
            {
                var value = this.Retrieve(x => x.TicketsForContentItemsMenuFlipFlopPosition);

                if (string.IsNullOrEmpty(value))
                {
                    value = DefaultTicketsForContentItemsMenuFlipFlopPosition;
                }

                return value;
            }
            set
            {
                this.Store(x => x.TicketsForContentItemsMenuFlipFlopPosition, value);
            }
        }

        public bool WithoutTheme
        {
            get
            {
                var value = this.Retrieve(x => x.WithoutTheme);


                return value;
            }
            set
            {
                this.Store(x => x.WithoutTheme, value);
            }
        }

        public int CreatedCustomerTicketState
        {
            get { return this.Retrieve(x => x.CreatedCustomerTicketState); }
            set { this.Store(x => x.CreatedCustomerTicketState, value); }
        }
    }
}