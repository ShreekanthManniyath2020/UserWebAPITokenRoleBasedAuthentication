namespace WebAPIJwtAuth.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal FinalPrice => DiscountPrice ?? Price;
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public CategoryDto? Category { get; set; }
        public BrandDto? Brand { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public UserDto? CreatedBy { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
        public Dictionary<string, string>? Metadata { get; set; }

        // Calculated properties
        public bool IsInStock => StockQuantity > 0;
        public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
        public decimal? DiscountPercentage => HasDiscount ?
            Math.Round(((Price - DiscountPrice!.Value) / Price) * 100, 2) : null;
    }
}
