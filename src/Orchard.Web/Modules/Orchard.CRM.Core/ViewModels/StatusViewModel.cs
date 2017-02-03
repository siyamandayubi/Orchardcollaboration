namespace Orchard.CRM.Core.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class StatusViewModel
    {
        public int StatusId { get; set; }
        public int OrderId { get; set; }

        public int? StatusTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}