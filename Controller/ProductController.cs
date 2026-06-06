using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] //  All endpoints require authentication by default
    public class ProductController : ControllerBase
    {
        public ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin only — matches requirement "Add a new product (Admin only)"
        [Authorize(Roles = "Admin")]
        [HttpPost("AddNewProduct")]
        public IActionResult AddNewProduct(ProductDOT productDto)
        {
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
                return BadRequest(ex.Message);
            }
        }

        // Any authenticated user can list products with pagination & filtering
        // Inherits class-level [Authorize] — no changes needed
        [HttpGet("ListAllProducts")]
        public IActionResult ListAllProducts(
            string? name,
            decimal? minPrice,
            decimal? maxPrice,
            int pageNumber = 1,
            int pageSize = 2)
        {
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

            return Ok(products);
        }

        // Removed [AllowAnonymous] — requirement says authenticated users only
        // Now inherits class-level [Authorize]
        [HttpGet("GetProductById")]
        public IActionResult GetProductById(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound("Product not found.");

            return Ok(new ProductDOT
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock
            });
        }

        // Admin only — matches requirement "Update product details (Admin only)"
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateProductById")]
        public IActionResult UpdateProductById(int id, ProductUpdateDOT productDto)
        {
            try
            {
                var existingProduct = _context.Products.Find(id);

                if (existingProduct == null)
                    return NotFound("Product not found.");

                existingProduct.Name = productDto.Name;
                existingProduct.Description = productDto.Description;
                existingProduct.Price = productDto.Price;
                existingProduct.Stock = productDto.Stock ?? 0;

                _context.SaveChanges();

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
                return BadRequest(ex.Message);
            }
        }
    }
}