using FluentValidation;
using Microsoft.Extensions.Logging;
using WebAPIJwtAuth.Application.DTOs;
using WebAPIJwtAuth.Application.Interfaces;
using WebAPIJwtAuth.Domain.Entities;
using WebAPIJwtAuth.Infrastructure.UnitOfWork;

namespace WebAPIJwtAuth.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateProductDto> _createProductValidator;
        private readonly IValidator<UpdateProductDto> _updateProductValidator;
        private readonly IValidator<CreateCategoryDto> _createCategoryValidator;
        private readonly IValidator<CreateBrandDto> _createBrandValidator;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IUnitOfWork unitOfWork,
            IValidator<CreateProductDto> createProductValidator,
            IValidator<UpdateProductDto> updateProductValidator,
            IValidator<CreateCategoryDto> createCategoryValidator,
            IValidator<CreateBrandDto> createBrandValidator,
            ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _createProductValidator = createProductValidator;
            _updateProductValidator = updateProductValidator;
            _createCategoryValidator = createCategoryValidator;
            _createBrandValidator = createBrandValidator;
            _logger = logger;
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<ProductDto?> GetProductBySkuAsync(string sku)
        {
            var product = await _unitOfWork.Products.GetBySkuAsync(sku);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<ProductResponseDto> GetProductsAsync(ProductQueryDto query)
        {
            var products = await _unitOfWork.Products.GetProductsWithDetailsAsync(
                query.Page,
                query.PageSize,
                query.Search,
                query.CategoryId,
                query.BrandId,
                query.IsFeatured,
                query.MinPrice,
                query.MaxPrice,
                query.SortBy,
                query.SortAsc);

            // Get counts (simplified - you'd want to implement a Count method)
            var allProducts = await _unitOfWork.Products.GetProductsWithDetailsAsync(
                page: 1,
                pageSize: int.MaxValue,
                search: query.Search,
                categoryId: query.CategoryId,
                brandId: query.BrandId,
                isFeatured: query.IsFeatured,
                minPrice: query.MinPrice,
                maxPrice: query.MaxPrice);

            var totalCount = allProducts.Count();

            var categories = await GetCategoriesAsync();
            var brands = await GetBrandsAsync();

            return new ProductResponseDto
            {
                Products = products.Select(MapToDto),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                Categories = categories,
                Brands = brands
            };
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 10)
        {
            var products = await _unitOfWork.Products.GetFeaturedProductsAsync(count);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetRelatedProductsAsync(Guid productId, int count = 5)
        {
            var products = await _unitOfWork.Products.GetRelatedProductsAsync(productId, count);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetActiveCategoriesAsync();
            return categories.Select(MapToDto);
        }

        public async Task<IEnumerable<BrandDto>> GetBrandsAsync()
        {
            var brands = await _unitOfWork.Brands.GetActiveBrandsAsync();
            return brands.Select(MapToDto);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid? createdBy = null)
        {
            await _createProductValidator.ValidateAndThrowAsync(dto);

            using var transaction = await BeginTransactionAsync();

            try
            {
                // Check SKU uniqueness
                var skuExists = await _unitOfWork.Products.SkuExistsAsync(dto.SKU);
                if (skuExists)
                {
                    throw new FluentValidation.ValidationException($"Product with SKU '{dto.SKU}' already exists.");
                }

                // Validate category and brand if provided
                if (dto.CategoryId.HasValue)
                {
                    var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId.Value);
                    if (category == null || !category.IsActive)
                    {
                        throw new FluentValidation.ValidationException($"Category with ID '{dto.CategoryId}' not found or inactive.");
                    }
                }

                if (dto.BrandId.HasValue)
                {
                    var brand = await _unitOfWork.Brands.GetByIdAsync(dto.BrandId.Value);
                    if (brand == null || !brand.IsActive)
                    {
                        throw new FluentValidation.ValidationException($"Brand with ID '{dto.BrandId}' not found or inactive.");
                    }
                }

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    SKU = dto.SKU.ToUpper(),
                    Price = dto.Price,
                    DiscountPrice = dto.DiscountPrice,
                    StockQuantity = dto.StockQuantity,
                    ImageUrl = dto.ImageUrl,
                    CategoryId = dto.CategoryId,
                    BrandId = dto.BrandId,
                    Weight = dto.Weight,
                    Dimensions = dto.Dimensions,
                    IsActive = true,
                    IsFeatured = dto.IsFeatured,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                // Add images if provided
                if (dto.Images != null && dto.Images.Any())
                {
                    var sortOrder = 0;
                    foreach (var imageDto in dto.Images)
                    {
                        product.Images.Add(new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            ImageUrl = imageDto.ImageUrl,
                            AltText = imageDto.AltText,
                            SortOrder = sortOrder++,
                            IsPrimary = imageDto.IsPrimary,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitTransactionAsync();

                _logger.LogInformation("Product created: {ProductId}, SKU: {SKU}", product.Id, product.SKU);

                return MapToDto(product);
            }
            catch
            {
                await transaction.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto)
        {
            await _updateProductValidator.ValidateAndThrowAsync(dto);

            using var transaction = await BeginTransactionAsync();

            try
            {
                var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
                if (product == null)
                {
                    throw new FluentValidation.ValidationException($"Product with ID '{id}' not found.");
                }

                // Check SKU uniqueness
                var skuExists = await _unitOfWork.Products.SkuExistsAsync(dto.SKU, id);
                if (skuExists)
                {
                    throw new FluentValidation.ValidationException($"Product with SKU '{dto.SKU}' already exists.");
                }

                // Validate category and brand if provided
                if (dto.CategoryId.HasValue && dto.CategoryId != product.CategoryId)
                {
                    var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId.Value);
                    if (category == null || !category.IsActive)
                    {
                        throw new FluentValidation.ValidationException($"Category with ID '{dto.CategoryId}' not found or inactive.");
                    }
                }

                if (dto.BrandId.HasValue && dto.BrandId != product.BrandId)
                {
                    var brand = await _unitOfWork.Brands.GetByIdAsync(dto.BrandId.Value);
                    if (brand == null || !brand.IsActive)
                    {
                        throw new FluentValidation.ValidationException($"Brand with ID '{dto.BrandId}' not found or inactive.");
                    }
                }

                // Update product
                product.Name = dto.Name;
                product.Description = dto.Description;
                product.SKU = dto.SKU.ToUpper();
                product.Price = dto.Price;
                product.DiscountPrice = dto.DiscountPrice;
                product.StockQuantity = dto.StockQuantity;
                product.ImageUrl = dto.ImageUrl;
                product.CategoryId = dto.CategoryId;
                product.BrandId = dto.BrandId;
                product.Weight = dto.Weight;
                product.Dimensions = dto.Dimensions;
                product.IsActive = dto.IsActive;
                product.IsFeatured = dto.IsFeatured;
                product.UpdatedAt = DateTime.UtcNow;

                // Update images if provided
                if (dto.Images != null)
                {
                    // Clear existing images
                    foreach (var image in product.Images.ToList())
                    {
                        //_unitOfWork.Context.Remove(image);
                    }

                    // Add new images
                    var sortOrder = 0;
                    foreach (var imageDto in dto.Images)
                    {
                        product.Images.Add(new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            ImageUrl = imageDto.ImageUrl,
                            AltText = imageDto.AltText,
                            SortOrder = sortOrder++,
                            IsPrimary = imageDto.IsPrimary,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitTransactionAsync();

                _logger.LogInformation("Product updated: {ProductId}", product.Id);

                return MapToDto(product);
            }
            catch
            {
                await transaction.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            using var transaction = await BeginTransactionAsync();

            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return false;
                }

                // Soft delete
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitTransactionAsync();

                _logger.LogInformation("Product deleted (soft): {ProductId}", product.Id);

                return true;
            }
            catch
            {
                await transaction.RollbackTransactionAsync();
                throw;
            }
        }

        // Categories
        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            return category != null ? MapToDto(category) : null;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            await _createCategoryValidator.ValidateAndThrowAsync(dto);

            // Check name uniqueness
            var nameExists = await _unitOfWork.Categories.NameExistsAsync(dto.Name);
            if (nameExists)
            {
                throw new FluentValidation.ValidationException($"Category with name '{dto.Name}' already exists.");
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category created: {CategoryId}, Name: {Name}", category.Id, category.Name);

            return MapToDto(category);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                throw new FluentValidation.ValidationException($"Category with ID '{id}' not found.");
            }

            // Check name uniqueness
            var nameExists = await _unitOfWork.Categories.NameExistsAsync(dto.Name, id);
            if (nameExists)
            {
                throw new FluentValidation.ValidationException($"Category with name '{dto.Name}' already exists.");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl;
            category.IsActive = dto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category updated: {CategoryId}", category.Id);

            return MapToDto(category);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            // Check if category has products
            var hasProducts = true;
            //await _unitOfWork.Context.Products
            //.AnyAsync(p => p.CategoryId == id && p.IsActive);

            if (hasProducts)
            {
                throw new FluentValidation.ValidationException("Cannot delete category that has active products.");
            }

            // Soft delete
            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category deleted (soft): {CategoryId}", category.Id);

            return true;
        }

        // Brands
        public async Task<BrandDto?> GetBrandByIdAsync(Guid id)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            return brand != null ? MapToDto(brand) : null;
        }

        public async Task<IEnumerable<BrandDto>> GetAllBrandsAsync()
        {
            var brands = await _unitOfWork.Brands.GetAllAsync();
            return brands.Select(MapToDto);
        }

        public async Task<BrandDto> CreateBrandAsync(CreateBrandDto dto)
        {
            await _createBrandValidator.ValidateAndThrowAsync(dto);

            // Check name uniqueness
            var nameExists = await _unitOfWork.Brands.NameExistsAsync(dto.Name);
            if (nameExists)
            {
                throw new FluentValidation.ValidationException($"Brand with name '{dto.Name}' already exists.");
            }

            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                LogoUrl = dto.LogoUrl,
                Website = dto.Website,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Brands.AddAsync(brand);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Brand created: {BrandId}, Name: {Name}", brand.Id, brand.Name);

            return MapToDto(brand);
        }

        public async Task<BrandDto> UpdateBrandAsync(Guid id, UpdateBrandDto dto)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            if (brand == null)
            {
                throw new FluentValidation.ValidationException($"Brand with ID '{id}' not found.");
            }

            // Check name uniqueness
            var nameExists = await _unitOfWork.Brands.NameExistsAsync(dto.Name, id);
            if (nameExists)
            {
                throw new FluentValidation.ValidationException($"Brand with name '{dto.Name}' already exists.");
            }

            brand.Name = dto.Name;
            brand.Description = dto.Description;
            brand.LogoUrl = dto.LogoUrl;
            brand.Website = dto.Website;
            brand.IsActive = dto.IsActive;
            brand.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Brands.UpdateAsync(brand);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Brand updated: {BrandId}", brand.Id);

            return MapToDto(brand);
        }

        public async Task<bool> DeleteBrandAsync(Guid id)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            if (brand == null)
            {
                return false;
            }

            // Check if brand has products
            var hasProducts = true;
            // await _unitOfWork.Context.Products
            //    .AnyAsync(p => p.BrandId == id && p.IsActive);

            if (hasProducts)
            {
                throw new FluentValidation.ValidationException("Cannot delete brand that has active products.");
            }

            // Soft delete
            brand.IsActive = false;
            brand.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Brands.UpdateAsync(brand);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Brand deleted (soft): {BrandId}", brand.Id);

            return true;
        }

        // Helper methods
        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                Category = product.Category != null ? MapToDto(product.Category) : null,
                Brand = product.Brand != null ? MapToDto(product.Brand) : null,
                Weight = product.Weight,
                Dimensions = product.Dimensions,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                AverageRating = product.AverageRating,
                TotalReviews = product.TotalReviews,
                CreatedBy = product.CreatedByUser != null ? new UserDto
                {
                    Id = product.CreatedByUser.Id,
                    Email = product.CreatedByUser.Email,
                    FirstName = product.CreatedByUser.FirstName,
                    LastName = product.CreatedByUser.LastName
                } : null,
                Images = product.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    SortOrder = i.SortOrder,
                    IsPrimary = i.IsPrimary,
                    AltText = i.AltText
                }).ToList(),
            };
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive
            };
        }

        private BrandDto MapToDto(Brand brand)
        {
            return new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                Website = brand.Website,
                IsActive = brand.IsActive
            };
        }

        private async Task<DisposableTransaction> BeginTransactionAsync()
        {
            var transaction = new DisposableTransaction(_unitOfWork);
            await transaction.BeginAsync();
            return transaction;
        }

        private class DisposableTransaction : IDisposable
        {
            private readonly IUnitOfWork _unitOfWork;
            private bool _committed;

            public DisposableTransaction(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            public async Task BeginAsync()
            {
                await _unitOfWork.BeginTransactionAsync();
            }

            public async Task CommitTransactionAsync()
            {
                await _unitOfWork.CommitTransactionAsync();
                _committed = true;
            }

            public async Task RollbackTransactionAsync()
            {
                await _unitOfWork.RollbackTransactionAsync();
            }

            public void Dispose()
            {
                if (!_committed && _unitOfWork.HasActiveTransaction)
                {
                    RollbackTransactionAsync().GetAwaiter().GetResult();
                }
            }
        }
    }
}