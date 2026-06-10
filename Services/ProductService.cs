using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace ECommerecAPI.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ILogger<ProductService> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult AddNewProduct(ProductDOT productDto, ModelStateDictionary modelState)
        {
            _logger.LogInformation("Admin adding new product: {Name}", productDto.Name);

            if (!modelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for AddNewProduct");
                return new BadRequestObjectResult(modelState);
            }

            try
            {
                var product = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    Stock = productDto.Stock ?? 0
                };

                _context.Products.Add(product);
                _context.SaveChanges();

                _logger.LogInformation("Product added successfully: {Name} with ID: {Id}", product.Name, product.Id);

                return new OkObjectResult(new ProductDOT
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
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public IActionResult ListAllProducts(
            string? name,
            decimal? minPrice,
            decimal? maxPrice,
            int pageNumber,
            int pageSize)
        {
            _logger.LogInformation("Listing products - Page: {Page}, Size: {Size}", pageNumber, pageSize);

            var query = _context.Products
                .Include(p => p.Reviews)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.Contains(name));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var products = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList()
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

            return new OkObjectResult(products);
        }

        public IActionResult GetProductById(int id)
        {
            _logger.LogInformation("Getting product by ID: {Id}", id);

            var product = _context.Products
                .Include(p => p.Reviews)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {Id}", id);
                return new NotFoundObjectResult("Product not found.");
            }

            _logger.LogInformation("Product found: {Name}", product.Name);

            return new OkObjectResult(new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.OverallRating
            });
        }

        public IActionResult UpdateProductById(int id, ProductUpdateDOT productDto, ModelStateDictionary modelState)
        {
            _logger.LogInformation("Admin updating product ID: {Id}", id);

            if (!modelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UpdateProductById");
                return new BadRequestObjectResult(modelState);
            }

            try
            {
                var existingProduct = _context.Products.Find(id);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Update failed - product not found: {Id}", id);
                    return new NotFoundObjectResult("Product not found.");
                }

                existingProduct.Name = productDto.Name;
                existingProduct.Description = productDto.Description;
                existingProduct.Price = productDto.Price;
                existingProduct.Stock = productDto.Stock ?? 0;

                _context.SaveChanges();

                _logger.LogInformation("Product updated successfully: {Id}", id);

                return new OkObjectResult(new ProductUpdateDOT
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
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public IActionResult DeleteProductById(int id)
        {
            _logger.LogInformation("Admin deleting product ID: {Id}", id);

            try
            {
                var product = _context.Products.Find(id);

                if (product == null)
                {
                    _logger.LogWarning("Delete failed - product not found: {Id}", id);
                    return new NotFoundObjectResult("Product not found.");
                }

                _context.Products.Remove(product);
                _context.SaveChanges();

                _logger.LogInformation("Product deleted successfully: {Id}", id);

                return new OkObjectResult("Product deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product ID: {Id}", id);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
