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