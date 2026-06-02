using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {

        ApplicationDbContext db = new ApplicationDbContext();


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

                db.Products.Add(product);
                db.SaveChanges();

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

        [HttpGet("ListAllProducts")]
        public IActionResult ListAllProducts(
    string? name,
    decimal? minPrice,
    decimal? maxPrice,
    int pageNumber = 1,
    int pageSize = 2)
        {
            var query = db.Products.AsQueryable();

            // Filter by name
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }

            // Filter by minimum price
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            // Filter by maximum price
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Pagination
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


        [HttpGet("GetProductById")]
        public IActionResult GetProductById(int id)
        {

            var pro = db.Products.Find(id);
            if (pro != null)
            {

                db.Users.ToList();
                return Ok(pro);
            }

            return NotFound("Product not found");

        }


        [HttpPut("UpdateProductById")]
        public IActionResult UpdateProductById(int id, ProductUpdateDOT productDto)
        {
            try
            {
                var existingProduct = db.Products.Find(id);

                if (existingProduct == null)
                {
                    return NotFound("Product not found");
                }

                existingProduct.Name = productDto.Name;
                existingProduct.Description = productDto.Description;
                existingProduct.Price = productDto.Price;
                existingProduct.Stock = productDto.Stock ?? 0;

                db.SaveChanges();

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



