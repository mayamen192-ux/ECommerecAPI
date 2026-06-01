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
        public IActionResult AddNewProduct(Product product)
        {
            try
            {
                if (product.Price <= 0)
                {
                    return BadRequest("Product price must be greater than zero.");
                }

                if (product.Stock < 0)
                {
                    return BadRequest("Stock cannot be negative.");
                }
                db.Products.Add(product);
                db.SaveChanges();
                return Ok("Product added Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
            [HttpGet("ListAllProducts")]
        public IActionResult ListAllProducts()
        {
            var products = db.Products.Select(p => new
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
        public IActionResult UpdateProductById(int id, Product product)
        {
            try
            {
                var existingProduct = db.Products.Find(id);

                if (existingProduct == null)
                {
                    return NotFound("Product not found");
                }

                if (product.Price <= 0)
                {
                    return BadRequest("Product price must be greater than zero.");
                }

                if (product.Stock < 0)
                {
                    return BadRequest("Stock cannot be negative.");
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;

                db.SaveChanges();

                return Ok("Product updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
    }



