using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPIJwtAuth.Application.DTOs;

namespace WebAPIJwtAuth.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductByIdAsync(Guid id);
        Task<ProductDto?> GetProductBySkuAsync(string sku);
        Task<ProductResponseDto> GetProductsAsync(ProductQueryDto query);
        Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<ProductDto>> GetRelatedProductsAsync(Guid productId, int count = 5);
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        Task<IEnumerable<BrandDto>> GetBrandsAsync();

        Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid? createdBy = null);
        Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(Guid id);

        // Categories
        Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(Guid id);

        // Brands
        Task<BrandDto?> GetBrandByIdAsync(Guid id);
        Task<IEnumerable<BrandDto>> GetAllBrandsAsync();
        Task<BrandDto> CreateBrandAsync(CreateBrandDto dto);
        Task<BrandDto> UpdateBrandAsync(Guid id, UpdateBrandDto dto);
        Task<bool> DeleteBrandAsync(Guid id);
    }
}
