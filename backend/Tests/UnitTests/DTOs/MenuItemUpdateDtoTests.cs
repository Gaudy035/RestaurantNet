using backend.DTOs.MenuItems;

namespace UnitTests.DTOs;

public class MenuItemUpdateDtoTests
{
    [Fact]
    public void IsEmpty_WhenNoFieldSet_IsTrue()
    {
        Assert.True(new MenuItemUpdateDto().IsEmpty);
    }

    [Fact]
    public void IsEmpty_WhenPriceSet_IsFalse()
    {
        Assert.False(new MenuItemUpdateDto { Price = 10.5m }.IsEmpty);
    }

    [Theory]
    [InlineData(1, null, null, null)]
    [InlineData(null, true, null, null)]
    [InlineData(null, null, true, null)]
    [InlineData(null, null, null, "url")]
    public void IsEmpty_WhenAnyFieldSet_IsFalse(int? categoryId, bool? isAvailable, bool? isPinned, string? imageUrl)
    {
        var dto = new MenuItemUpdateDto
        {
            CategoryId = categoryId,
            IsAvailable = isAvailable,
            IsPinned = isPinned,
            ImageUrl = imageUrl
        };

        Assert.False(dto.IsEmpty);
    }
}
