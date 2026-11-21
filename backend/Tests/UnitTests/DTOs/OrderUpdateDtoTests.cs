using backend.Data.Entities;
using backend.DTOs.Orders;

namespace UnitTests.DTOs;

public class OrderUpdateDtoTests
{
    [Fact]
    public void IsEmpty_WhenNoFieldSet_IsTrue()
    {
        Assert.True(new OrderUpdateDto().IsEmpty);
    }

    [Theory]
    [InlineData(PaymentMethod.Card, null, null, null, null, null)]
    [InlineData(null, true, null, null, null, null)]
    [InlineData(null, null, OrderStatus.Ready, null, null, null)]
    [InlineData(null, null, null, "123 123 123", null, null)]
    [InlineData(null, null, null, null, "City", null)]
    [InlineData(null, null, null, null, null, "Address")]
    public void IsEmpty_WhenAnyFieldSet_IsFalse(
        PaymentMethod? paymentMethod,
        bool? isPaid,
        OrderStatus? status,
        string? phoneNumber,
        string? city,
        string? address)
    {
        var dto = new OrderUpdateDto
        {
            PaymentMethod = paymentMethod,
            IsPaid = isPaid,
            Status = status,
            PhoneNumber = phoneNumber,
            City = city,
            Address = address
        };

        Assert.False(dto.IsEmpty);
    }

    [Theory]
    [InlineData("   ", null, null)]
    [InlineData(null, "   ", null)]
    [InlineData(null, null, "   ")]
    public void IsEmpty_WhenOnlyWhitespaceStrings_IsTrue(
        string? phoneNumber,
        string? city,
        string? address)
    {
        var dto = new OrderUpdateDto
        {
            PhoneNumber = phoneNumber,
            City = city,
            Address = address
        };

        Assert.True(dto.IsEmpty);
    }
}
