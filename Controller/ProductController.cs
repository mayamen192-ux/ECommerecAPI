using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            ILogger<ProductController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Admin only
        [Authorize(Roles = "Admin")]
        [HttpPost("AddNewProduct")]
        public IActionResult AddNewProduct([FromBody] ProductDOT productDto)
        {
            _logger.LogInformation("Admin adding new product: {Name}", productDto.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for AddNewProduct");
                return BadRequest(ModelState);
            }

            try
            {
                Product product = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    Stock = productDto.Stock ?? 0
                };

                _context.Products.Add(product);
                _context.SaveChanges();

                _logger.LogInformation("Product added successfully: {Name} with ID: {Id}", product.Name, product.Id);

                return Ok(new ProductDOT
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product: {Name}", productDto.Name);
                return BadRequest(ex.Message);
            }
        }

        // Any authenticated user
        [HttpGet("ListAllProducts")]
        public IActionResult ListAllProducts(
            string? name,
            decimal? minPrice,
            decimal? maxPrice,
            int pageNumber = 1,
            int pageSize = 2)
        {
            _logger.LogInformation("Listing products - Page: {Page}, Size: {Size}", pageNumber, pageSize);

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.Contains(name));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var products = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Stock,
                    p.OverallRating
                })
                .ToList();

            _logger.LogInformation("Returned {Count} products", products.Count);

            return Ok(products);
        }

        // Any authenticated user
        [HttpGet("GetProductById")]
        public IActionResult GetProductById(int id)
        {
            _logger.LogInformation("Getting product by ID: {Id}", id);

            var product = _context.Products.Find(id);

            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {Id}", id);
                return NotFound("Product not found.");
            }

            _logger.LogInformation("Product found: {Name}", product.Name);

            return Ok(new ProductDOT
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock
            });
        }

        // Admin only
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateProductById")]
        public IActionResult UpdateProductById(int id, [FromBody] ProductUpdateDOT productDto)
        {
            _logger.LogInformation("Admin updating product ID: {Id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UpdateProductById");
                return BadRequest(ModelState);
            }

            try
            {
                var existingProduct = _context.Products.Find(id);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Update failed - product not found: {Id}", id);
                    return NotFound("Product not found.");
                }

                existingProduct.Name = productDto.Name;
                existingProduct.Description = productDto.Description;
                existingProduct.Price = productDto.Price;
                existingProduct.Stock = productDto.Stock ?? 0;

                _context.SaveChanges();

                _logger.LogInformation("Product updated successfully: {Id}", id);

                return Ok(new ProductUpdateDOT
                {
                    Name = existingProduct.Name,
                    Description = existingProduct.Description,
                    Price = existingProduct.Price,
                    Stock = existingProduct.Stock
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        // Admin only - Delete product
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteProductById")]
        public IActionResult DeleteProductById(int id)
        {
            _logger.LogInformation("Admin deleting product ID: {Id}", id);

            try
            {
                var product = _context.Products.Find(id);

                if (product == null)
                {
                    _logger.LogWarning("Delete failed - product not found: {Id}", id);
                    return NotFound("Product not found.");
                }

                _context.Products.Remove(product);
                _context.SaveChanges();

                _logger.LogInformation("Product deleted successfully: {Id}", id);

                return Ok("Product deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}