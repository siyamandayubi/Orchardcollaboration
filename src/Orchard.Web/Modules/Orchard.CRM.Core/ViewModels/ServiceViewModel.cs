namespace Orchard.CRM.Core.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ServiceViewModel
    {
        public int ServiceId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}