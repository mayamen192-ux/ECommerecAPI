using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace ECommerecAPI.Controller;
    

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        ApplicationDbContext db = new ApplicationDbContext();

       

[HttpPost("RegisterUser")]
    public IActionResult RegisterUser([FromBody] UserRegister userDto)
    {
        // Validate email format
        try
        {
            var email = new MailAddress(userDto.email);
        }
        catch
        {
            return BadRequest("Invalid email format.");
        }

        // Check unique email
        var existingUser = db.Users
            .FirstOrDefault(u => u.email == userDto.email);

        if (existingUser != null)
        {
            return BadRequest("Email already exists.");
        }

        User u = new User
        {
            name = userDto.name,
            email = userDto.email,
            phone = userDto.phone,
            password = HashPassword(userDto.password),
            role = "User",
            createdAt = DateTime.Now
        };

        db.Users.Add(u);
        db.SaveChanges();

        return Ok("User registered successfully with ID = " + u.Id);
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

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var users = db.Users.ToList();


            var OutPutUsers = new List<UserOutput>();



            foreach (var user in users)
            {
                OutPutUsers.Add(

                    new UserOutput
                    {
                        name = user.name,
                        email = user.email,
                        phone = user.phone
                    });

            }
            return Ok(OutPutUsers);
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
