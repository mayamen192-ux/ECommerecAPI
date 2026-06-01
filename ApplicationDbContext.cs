using ECommerecAPI.Controller;
using ECommerecAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace ECommerecAPI
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        //connection t database
        options.UseSqlServer(
@"Server=(localdb)\MSSQLLocalDB;Database=ECommerceAPI;Trusted_Connection=True;"
);
    }


    //Registered classes
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<OrderProducts> orderProducts { get; set; }

}
}
    
    

