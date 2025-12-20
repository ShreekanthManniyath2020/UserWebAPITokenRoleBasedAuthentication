using WebAPIJwtAuth.Application.DTOs;

namespace LoginMCVWebAPI.Services.Interfaces
{
    public interface IProductApiClient
    {
        Task<ApiResponse<ProductResponseDto>> GetProductsAsync(ProductQueryDto query);

        Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id);

        Task<ApiResponse<ProductDto>> GetProductBySkuAsync(string sku);

        Task<ApiResponse<IEnumerable<ProductDto>>> GetFeaturedProductsAsync(int count = 10);

        Task<ApiResponse<IEnumerable<ProductDto>>> GetRelatedProductsAsync(Guid productId, int count = 5);

        Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoriesAsync();

        Task<ApiResponse<IEnumerable<BrandDto>>> GetBrandsAsync();

        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto);

        Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid id, UpdateProductDto dto);

        Task<ApiResponse<bool>> DeleteProductAsync(Guid id);
    }
}