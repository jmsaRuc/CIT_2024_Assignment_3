using Microsoft.EntityFrameworkCore;

using Assignment_3_Api.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CategoryDb>(opt => opt.UseInMemoryDatabase("CategoryList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "Assignment_3_API";
    config.Title = "Assignment_3_API v1";
    config.Version = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Assignment_3_API";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapGet("/categories", async (CategoryDb db) =>
    await db.Categories.ToListAsync());

app.MapGet("/categories/complete", async (CategoryDb db) =>
    await db.Categories.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/categories/{cid}", async (int cid, CategoryDb db) =>
    await db.Categories.FindAsync(cid)
        is Category category
            ? Results.Ok(category)
            : Results.NotFound());

app.MapPost("/categories", async (Category category, CategoryDb db) =>
{
    db.Categories.Add(category);
    await db.SaveChangesAsync();

    return Results.Created($"/categories/{category.Cid}", category);
});

app.MapPut("/categories/{cid}", async (int cid, Category inputTodo, CategoryDb db) =>
{
    var category = await db.Categories.FindAsync(cid);

    if (category is null) return Results.NotFound();

    category.Name = inputTodo.Name;
    category.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/categories/{cid}", async (int cid, CategoryDb db) =>
{
    if (await db.Categories.FindAsync(cid) is Category category)
    {
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();