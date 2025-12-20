using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIJwtAuth.Application.DTOs;
using WebAPIJwtAuth.Application.Interfaces;

namespace WebAPIJwtAuthDotNet9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BrandsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<BrandsController> _logger;

        public BrandsController(
            IProductService productService,
            ILogger<BrandsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBrands()
        {
            try
            {
                var brands = await _productService.GetAllBrandsAsync();
                return Ok(brands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brands");
                return StatusCode(500, new { message = "An error occurred while fetching brands." });
            }
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBrand(Guid id)
        {
            try
            {
                var brand = await _productService.GetBrandByIdAsync(id);
                if (brand == null)
                {
                    return NotFound(new { message = "Brand not found." });
                }

                return Ok(brand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand with ID: {BrandId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching the brand." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto dto)
        {
            try
            {
                var brand = await _productService.CreateBrandAsync(dto);
                return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, brand);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = "Validation failed", errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                return StatusCode(500, new { message = "An error occurred while creating the brand." });
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandDto dto)
        {
            try
            {
                var brand = await _productService.UpdateBrandAsync(id, dto);
                return Ok(brand);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = "Validation failed", errors = ex.Errors });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Brand not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand with ID: {BrandId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the brand." });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBrand(Guid id)
        {
            try
            {
                var result = await _productService.DeleteBrandAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Brand not found." });
                }

                return NoContent();
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand with ID: {BrandId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the brand." });
            }
        }
    }
}