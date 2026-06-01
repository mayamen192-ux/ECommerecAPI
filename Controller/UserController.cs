using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        ApplicationDbContext db = new ApplicationDbContext();
        [HttpPost("AddNewUser")]
        public IActionResult AddNewUser(User user)
        {

            try
            {
                if (db.Users.Any(u => u.email == user.email))
                {
                    return BadRequest("Email already exists.");
                }
                user.password = HashPassword(user.password);
                db.Users.Add(user);
                db.SaveChanges();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
      
        [HttpPost("LoginUser")]
public IActionResult LoginUser(string email, string password)
{
    try
    {
                string hashedPassword = HashPassword(password);
                var user = db.Users.FirstOrDefault(u =>
            u.email == email &&
            u.password == hashedPassword);

        if (user == null)
        {
            return NotFound("Invalid email or password.");
        }

        return Ok(user);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.ToString());
    }
}

            [HttpGet("ListAllUsers")]
        public IActionResult ListAllUsers()
        {
            var users = db.Users.ToList();
            return Ok(users);

        }

        [HttpGet("GetUserById")]
        public IActionResult GetUserById(int id)
        {

            var user = db.Users.Find(id);
            if (user != null)
            {

                db.Users.ToList();
                return Ok(user);
            }

          return NotFound("User not found"); 

        }
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);           // returns 64-char hex string
        }
       
    }
}