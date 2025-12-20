using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIJwtAuth.Application.DTOs;
using WebAPIJwtAuth.Application.Interfaces;

namespace WebAPIJwtAuthDotNet9.Controllers
{
    // Separate controllers for Categories and Brands
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            IProductService productService,
            ILogger<CategoriesController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _productService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, new { message = "An error occurred while fetching categories." });
            }
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategory(Guid id)
        {
            try
            {
                var category = await _productService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found." });
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category with ID: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching the category." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                var category = await _productService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = "Validation failed", errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "An error occurred while creating the category." });
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                var category = await _productService.UpdateCategoryAsync(id, dto);
                return Ok(category);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = "Validation failed", errors = ex.Errors });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Category not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the category." });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var result = await _productService.DeleteCategoryAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Category not found." });
                }

                return NoContent();
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the category." });
            }
        }
    }
}