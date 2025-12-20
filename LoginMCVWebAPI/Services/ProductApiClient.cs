using LoginMCVWebAPI.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebAPIJwtAuth.Application.DTOs;

namespace LoginMCVWebAPI.Services
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthApiClient _authApiClient;
        private readonly ILogger<ProductApiClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TokenKey = "JwtToken";
        private const string RefreshTokenKey = "RefreshToken";

        public ProductApiClient(
            HttpClient httpClient,
            IAuthApiClient authApiClient,
            ILogger<ProductApiClient> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _authApiClient = authApiClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<ProductResponseDto>> GetProductsAsync(ProductQueryDto query)
        {
            try
            {
                // Build query string
                var queryParams = new List<string>
            {
                $"page={query.Page}",
                $"pageSize={query.PageSize}",
                $"sortBy={query.SortBy}",
                $"sortAsc={query.SortAsc}"
            };

                if (!string.IsNullOrEmpty(query.Search))
                    queryParams.Add($"search={Uri.EscapeDataString(query.Search)}");

                if (query.CategoryId.HasValue)
                    queryParams.Add($"categoryId={query.CategoryId}");

                if (query.BrandId.HasValue)
                    queryParams.Add($"brandId={query.BrandId}");

                if (query.IsFeatured.HasValue)
                    queryParams.Add($"isFeatured={query.IsFeatured}");

                if (query.MinPrice.HasValue)
                    queryParams.Add($"minPrice={query.MinPrice}");

                if (query.MaxPrice.HasValue)
                    queryParams.Add($"maxPrice={query.MaxPrice}");

                var queryString = string.Join("&", queryParams);
                var url = $"api/products?{queryString}";

                var response = await _httpClient.GetAsync(url);
                return await HandleResponse<ProductResponseDto>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return new ApiResponse<ProductResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching products."
                };
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/{id}");
                return await HandleResponse<ProductDto>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID: {ProductId}", id);
                return new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching the product."
                };
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductBySkuAsync(string sku)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/sku/{sku}");
                return await HandleResponse<ProductDto>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with SKU: {SKU}", sku);
                return new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching the product."
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetFeaturedProductsAsync(int count = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/featured?count={count}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<IEnumerable<ProductDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<IEnumerable<ProductDto>>
                    {
                        Success = true,
                        Data = products!
                    };
                }

                return new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "Failed to fetch featured products."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products");
                return new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching featured products."
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetRelatedProductsAsync(Guid productId, int count = 5)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/{productId}/related?count={count}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<IEnumerable<ProductDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<IEnumerable<ProductDto>>
                    {
                        Success = true,
                        Data = products!
                    };
                }

                return new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "Failed to fetch related products."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related products for product ID: {ProductId}", productId);
                return new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching related products."
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/products/categories");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<IEnumerable<CategoryDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<IEnumerable<CategoryDto>>
                    {
                        Success = true,
                        Data = categories!
                    };
                }

                return new ApiResponse<IEnumerable<CategoryDto>>
                {
                    Success = false,
                    Message = "Failed to fetch categories."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return new ApiResponse<IEnumerable<CategoryDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching categories."
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<BrandDto>>> GetBrandsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/products/brands");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var brands = JsonSerializer.Deserialize<IEnumerable<BrandDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<IEnumerable<BrandDto>>
                    {
                        Success = true,
                        Data = brands!
                    };
                }

                return new ApiResponse<IEnumerable<BrandDto>>
                {
                    Success = false,
                    Message = "Failed to fetch brands."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands");
                return new ApiResponse<IEnumerable<BrandDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching brands."
                };
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto)
        {
            try
            {
                var token = GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Authentication required."
                    };
                }

                var content = new StringContent(
                    JsonSerializer.Serialize(dto),
                    Encoding.UTF8,
                    "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "api/products")
                {
                    Content = content
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                return await HandleResponse<ProductDto>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the product."
                };
            }
        }

        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid id, UpdateProductDto dto)
        {
            try
            {
                var token = GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Authentication required."
                    };
                }

                var content = new StringContent(
                    JsonSerializer.Serialize(dto),
                    Encoding.UTF8,
                    "application/json");

                var request = new HttpRequestMessage(HttpMethod.Put, $"api/products/{id}")
                {
                    Content = content
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                return await HandleResponse<ProductDto>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
                return new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "An error occurred while updating the product."
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(Guid id)
        {
            try
            {
                var token = GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Authentication required."
                    };
                }

                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/products/{id}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool> { Success = true, Data = true };
                }

                return await HandleResponse<bool>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the product."
                };
            }
        }

        private string? GetTokenAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Session?.GetString(TokenKey);
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = result!
                    };
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing response: {Content}", content);
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = "Error processing response"
                    };
                }
            }
            else
            {
                try
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogWarning("API error response: {StatusCode} - {Message}",
                        response.StatusCode, error?.Message);

                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = error?.Message ?? $"Error: {response.StatusCode}"
                    };
                }
                catch
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = $"Error: {response.StatusCode}"
                    };
                }
            }
        }
    }
}