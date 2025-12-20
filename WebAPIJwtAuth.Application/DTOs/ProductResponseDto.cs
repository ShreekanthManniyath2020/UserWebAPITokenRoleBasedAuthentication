namespace WebAPIJwtAuth.Application.DTOs
{
    public class ProductResponseDto
    {
        public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public IEnumerable<BrandDto> Brands { get; set; } = new List<BrandDto>();
    }
}
