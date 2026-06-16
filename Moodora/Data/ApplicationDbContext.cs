using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moodora.Models;

namespace Moodora.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<MoodCategory> MoodCategories => Set<MoodCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ProductMoodCategory> ProductMoodCategories => Set<ProductMoodCategory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MoodCategory>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.DeleteDate);
        });

        builder.Entity<Product>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Ignore(x => x.SelectedMoodCategoryIds);
        });
        builder.Entity<ProductMoodCategory>(entity =>
        {
            entity.HasKey(x => new { x.ProductId, x.MoodCategoryId });

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductMoodCategories)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.MoodCategory)
                 .WithMany(x => x.ProductMoodCategories)
                .HasForeignKey(x => x.MoodCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Order>(entity =>
        {
            entity.Property(x => x.OrderNumber).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(40).IsRequired();
            entity.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
            entity.Property(x => x.City).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(500).IsRequired();
            entity.Property(x => x.AdditionalComment).HasMaxLength(1000);
            entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.PaymentMethod).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ProductImage).HasMaxLength(1000);
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.TransactionId).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.TransactionId).IsUnique();
            entity.Property(x => x.CardLastFourDigits).HasMaxLength(4);
            entity.Property(x => x.CardBrand).HasMaxLength(80);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.Order)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Cart>(entity =>
        {
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}