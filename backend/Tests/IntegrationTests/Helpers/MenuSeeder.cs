using backend.Data;
using backend.Data.Entities;

namespace IntegrationTests.Helpers;

public static class MenuSeeder
{
    public static async Task<Category> SeedCategory(
        AppDbContext context, 
        string categoryName = "TestCategory"
    )
    {
        var newCategory = new Category
        {
            CategoryName = categoryName
        };

        context.Categories.Add(newCategory);
        await context.SaveChangesAsync();
        
        return newCategory;
    }
}