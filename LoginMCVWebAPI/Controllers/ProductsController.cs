using LoginMCVWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIJwtAuth.Application.DTOs;

namespace LoginMCVWebAPI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductApiClient _productApiClient;
        private readonly IAuthApiClient _authApiClient;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductApiClient productApiClient,
            IAuthApiClient authApiClient,
            ILogger<ProductsController> logger)
        {
            _productApiClient = productApiClient;
            _authApiClient = authApiClient;
            _logger = logger;
        }

        // GET: /products
        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 12,
            string? search = null,
            Guid? categoryId = null,
            Guid? brandId = null,
            bool? featured = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sortBy = "name",
            bool sortAsc = true)
        {
            try
            {
                var query = new ProductQueryDto
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    CategoryId = categoryId,
                    BrandId = brandId,
                    IsFeatured = featured,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    SortBy = sortBy,
                    SortAsc = sortAsc
                };

                var result = await _productApiClient.GetProductsAsync(query);

                if (result.Success)
                {
                    ViewBag.CurrentPage = page;
                    ViewBag.SearchTerm = search;
                    ViewBag.CategoryId = categoryId;
                    ViewBag.BrandId = brandId;
                    ViewBag.SortBy = sortBy;
                    ViewBag.SortAsc = sortAsc;

                    return View(result.Data);
                }

                ViewBag.ErrorMessage = result.Message;
                return View(new ProductResponseDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products page");
                ViewBag.ErrorMessage = "An error occurred while loading products.";
                return View(new ProductResponseDto());
            }
        }

        // GET: /products/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var productResult = await _productApiClient.GetProductByIdAsync(id);

                if (!productResult.Success || productResult.Data == null)
                {
                    TempData["ErrorMessage"] = productResult.Message ?? "Product not found.";
                    return RedirectToAction("Index");
                }

                // Get related products
                var relatedResult = await _productApiClient.GetRelatedProductsAsync(id, 4);
                if (relatedResult.Success)
                {
                    ViewBag.RelatedProducts = relatedResult.Data;
                }

                return View(productResult.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details for ID: {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading product details.";
                return RedirectToAction("Index");
            }
        }

        // GET: /products/create
        [HttpGet("create")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        // POST: /products/create
        [HttpPost("create")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                return View(model);
            }

            try
            {
                var result = await _productApiClient.CreateProductAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction("Details", new { id = result.Data.Id });
                }

                ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create product.");
                await LoadDropdownData();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the product.");
                await LoadDropdownData();
                return View(model);
            }
        }

        // GET: /products/edit/{id}
        [HttpGet("edit/{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var result = await _productApiClient.GetProductByIdAsync(id);

                if (!result.Success || result.Data == null)
                {
                    TempData["ErrorMessage"] = result.Message ?? "Product not found.";
                    return RedirectToAction("Index");
                }

                await LoadDropdownData();

                var model = new UpdateProductDto
                {
                    Name = result.Data.Name,
                    Description = result.Data.Description,
                    SKU = result.Data.SKU,
                    Price = result.Data.Price,
                    DiscountPrice = result.Data.DiscountPrice,
                    StockQuantity = result.Data.StockQuantity,
                    ImageUrl = result.Data.ImageUrl,
                    CategoryId = result.Data.CategoryId,
                    BrandId = result.Data.BrandId,
                    Weight = result.Data.Weight,
                    Dimensions = result.Data.Dimensions,
                    IsActive = result.Data.IsActive,
                    IsFeatured = result.Data.IsFeatured,
                    Images = result.Data.Images.Select(i => new ProductImageDto
                    {
                        ImageUrl = i.ImageUrl,
                        AltText = i.AltText,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder
                    }).ToList(),
                    Metadata = result.Data.Metadata
                };

                ViewBag.ProductId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product edit page for ID: {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the product.";
                return RedirectToAction("Index");
            }
        }

        // POST: /products/edit/{id}
        [HttpPost("edit/{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateProductDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                ViewBag.ProductId = id;
                return View(model);
            }

            try
            {
                var result = await _productApiClient.UpdateProductAsync(id, model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction("Details", new { id });
                }

                ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update product.");
                await LoadDropdownData();
                ViewBag.ProductId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the product.");
                await LoadDropdownData();
                ViewBag.ProductId = id;
                return View(model);
            }
        }

        // POST: /products/delete/{id}
        [HttpPost("delete/{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _productApiClient.DeleteProductAsync(id);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Product deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message ?? "Failed to delete product.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the product.";
                return RedirectToAction("Index");
            }
        }

        // GET: /products/featured
        [HttpGet("featured")]
        public async Task<IActionResult> Featured()
        {
            try
            {
                var result = await _productApiClient.GetFeaturedProductsAsync(12);

                if (result.Success)
                {
                    return View(result.Data);
                }

                ViewBag.ErrorMessage = result.Message;
                return View(new List<ProductDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading featured products");
                ViewBag.ErrorMessage = "An error occurred while loading featured products.";
                return View(new List<ProductDto>());
            }
        }

        // Helper method to load dropdown data
        private async Task LoadDropdownData()
        {
            var categoriesResult = await _productApiClient.GetCategoriesAsync();
            if (categoriesResult.Success)
            {
                ViewBag.Categories = categoriesResult.Data;
            }

            var brandsResult = await _productApiClient.GetBrandsAsync();
            if (brandsResult.Success)
            {
                ViewBag.Brands = brandsResult.Data;
            }
        }
    }
}
