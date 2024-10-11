using System;

using Microsoft.EntityFrameworkCore;
namespace Assignment_3_Api.Models;

class CategoryDb : DbContext
{

    public CategoryDb(DbContextOptions<CategoryDb> options)
    : base(options) { 
    }

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasKey(x => x.Cid);
        base.OnModelCreating(modelBuilder);
    }
}
