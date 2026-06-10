using ECommerecAPI.DTOs;
using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AddNewProduct")]
        public IActionResult AddNewProduct([FromBody] ProductDOT productDto)
            => _productService.AddNewProduct(productDto, ModelState);

        [HttpGet("ListAllProducts")]
        public IActionResult ListAllProducts(
            string? name,
            decimal? minPrice,
            decimal? maxPrice,
            int pageNumber = 1,
            int pageSize = 2)
            => _productService.ListAllProducts(name, minPrice, maxPrice, pageNumber, pageSize);

        [HttpGet("GetProductById")]
        public IActionResult GetProductById(int id)
            => _productService.GetProductById(id);

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateProductById")]
        public IActionResult UpdateProductById(int id, [FromBody] ProductUpdateDOT productDto)
            => _productService.UpdateProductById(id, productDto, ModelState);

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteProductById")]
        public IActionResult DeleteProductById(int id)
            => _productService.DeleteProductById(id);
    }
}