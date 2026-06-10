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
        {
            var result = _productService.AddNewProduct(productDto, ModelState);
            return ToActionResult(result);
        }

        [HttpGet("ListAllProducts")]
        public IActionResult ListAllProducts(
            string? name,
            decimal? minPrice,
            decimal? maxPrice,
            int pageNumber = 1,
            int pageSize = 2)
        {
            var result = _productService.ListAllProducts(name, minPrice, maxPrice, pageNumber, pageSize);
            return ToActionResult(result);
        }

        [HttpGet("GetProductById")]
        public IActionResult GetProductById(int id)
        {
            var result = _productService.GetProductById(id);
            return ToActionResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateProductById")]
        public IActionResult UpdateProductById(int id, [FromBody] ProductUpdateDOT productDto)
        {
            var result = _productService.UpdateProductById(id, productDto, ModelState);
            return ToActionResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteProductById")]
        public IActionResult DeleteProductById(int id)
        {
            var result = _productService.DeleteProductById(id);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult(object result)
        {
            var statusCode = (int)result.GetType().GetProperty("statusCode")!.GetValue(result)!;
            return statusCode switch
            {
                200 => Ok(result),
                400 => BadRequest(result),
                401 => Unauthorized(result),
                403 => Forbid(),
                404 => NotFound(result),
                _ => StatusCode(statusCode, result)
            };
        }
    }
}