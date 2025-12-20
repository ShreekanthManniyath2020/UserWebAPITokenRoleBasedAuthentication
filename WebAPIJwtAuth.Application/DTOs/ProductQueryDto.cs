namespace WebAPIJwtAuth.Application.DTOs
{
    public class ProductQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public bool? IsFeatured { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } = "name";
        public bool SortAsc { get; set; } = true;
    }
}
