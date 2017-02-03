namespace Orchard.CRM.Core.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class PriorityViewModel
    {
        public int PriorityId { get; set; }
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}