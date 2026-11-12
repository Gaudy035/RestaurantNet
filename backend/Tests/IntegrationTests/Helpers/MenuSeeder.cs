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

    public static async Task<MenuItem> SeedMenuItem(
        AppDbContext context,
        int categoryId, 
        string name = "Test",
        string imageUrl = "TestUrl",
        bool isAvailable = true,
        bool isPinned = false,
        decimal price = 25
    )
    {
        var newMenuItem = new MenuItem
        {
            CategoryId = categoryId,
            Name = name,
            ImageUrl = imageUrl,
            IsAvailable = isAvailable,
            IsPinned = isPinned,
            Price = price
        };

        context.MenuItems.Add(newMenuItem);
        await context.SaveChangesAsync();
        
        return newMenuItem;
    }

    public static async Task<CartItem> SeedCartItem(
        AppDbContext context,
        int userId,
        int itemId,
        int quantity
    )
    {
        var newCartItem = new CartItem
        {
            UserId = userId,
            ItemId = itemId,
            Quantity = quantity
        };

        context.CartItems.Add(newCartItem);
        await context.SaveChangesAsync();

        return newCartItem;
    }
}