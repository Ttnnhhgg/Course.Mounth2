using System.ComponentModel.DataAnnotations;
namespace Shared.Models.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        [Required]
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
