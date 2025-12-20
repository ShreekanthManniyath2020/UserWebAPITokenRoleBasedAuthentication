namespace WebAPIJwtAuth.Application.DTOs
{
    public class UpdateBrandDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }
    }
}