namespace WebAPIJwtAuth.Application.DTOs
{
    public class CreateProductDto
    {
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
        public bool IsFeatured { get; set; }
        public List<ProductImageDto>? Images { get; set; }
    }
}
