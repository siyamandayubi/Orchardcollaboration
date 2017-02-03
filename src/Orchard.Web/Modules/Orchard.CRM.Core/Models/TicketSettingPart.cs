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