namespace WebAPIJwtAuth.Application.DTOs
{
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
        public string? AltText { get; set; }
    }
}
