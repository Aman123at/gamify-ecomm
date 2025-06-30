using GamifyApi.Models;
using Microsoft.EntityFrameworkCore;

public class GamifyDbContext : DbContext
{
    public GamifyDbContext(DbContextOptions<GamifyDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductImages> ProductImages => Set<ProductImages>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartProduct> CartProducts => Set<CartProduct>();
    public DbSet<Order> Orders => Set<Order>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder.Entity<Address>().ToTable("Addresses");
        modelBuilder.Entity<Category>().ToTable("Categories");
        modelBuilder.Entity<ProductImages>().ToTable("ProductImages");
        modelBuilder.Entity<Cart>().ToTable("Carts");
        modelBuilder.Entity<CartProduct>().ToTable("CartProducts");
        modelBuilder.Entity<Order>().ToTable("Orders");

        // Configure relationships if needed
        modelBuilder.Entity<Product>()
            .HasOne<Category>()
            .WithOne()
            .HasForeignKey<Product>(c => c.CategoryId);

        modelBuilder.Entity<Product>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<Product>(p => p.OwnerId);

        modelBuilder.Entity<Address>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<Address>(a => a.UserId);

        modelBuilder.Entity<ProductImages>()
            .HasOne<Product>()
            .WithOne()
            .HasForeignKey<ProductImages>(pi => pi.ProductId);

        modelBuilder.Entity<Cart>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<Cart>(c => c.UserId);

        modelBuilder.Entity<CartProduct>()
            .HasOne<Cart>()
            .WithOne()
            .HasForeignKey<CartProduct>(cp => cp.CartId);

        modelBuilder.Entity<CartProduct>()
            .HasOne<Product>()
            .WithOne()
            .HasForeignKey<CartProduct>(cp => cp.ProductId);

        modelBuilder.Entity<Order>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<Order>(o => o.UserId);

        modelBuilder.Entity<Order>()
            .HasOne<Address>()
            .WithOne()
            .HasForeignKey<Order>(o => o.AddressId);
    }
}