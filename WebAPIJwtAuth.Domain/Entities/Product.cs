namespace WebAPIJwtAuth.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        // Rating properties
        public double? AverageRating { get; set; }

        public int? TotalReviews { get; set; }

        // Navigation properties
        public Category? Category { get; set; }

        public Brand? Brand { get; set; }
        public User? CreatedByUser { get; set; }
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    }
}