namespace WebAPIJwtAuth.Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
        public string? AltText { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Product Product { get; set; } = null!;
    }
}