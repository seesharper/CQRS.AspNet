using System;
using Xunit;

public class TypeConversionHelperTests
{
    [Theory]
    [InlineData("123", typeof(int), 123)]
    [InlineData("123", typeof(int?), 123)]
    [InlineData(null, typeof(int?), null)]
    [InlineData("true", typeof(bool), true)]
    [InlineData("false", typeof(bool?), false)]
    [InlineData(null, typeof(bool?), null)]
    [InlineData("123.45", typeof(double), 123.45)]
    [InlineData("123.45", typeof(double?), 123.45)]
    [InlineData("2023-12-25", typeof(DateTime), "2023-12-25")]
    [InlineData("2023-12-25", typeof(DateTime?), "2023-12-25")]
    [InlineData(null, typeof(DateTime?), null)]
    [InlineData("c", typeof(char), 'c')]
    [InlineData("c", typeof(char?), 'c')]
    [InlineData("EnumValue", typeof(TestEnum), TestEnum.EnumValue)]
    [InlineData("EnumValue", typeof(TestEnum?), TestEnum.EnumValue)]
    public void ConvertTo_ValidInput_ReturnsExpectedResult(string input, Type targetType, object expected)
    {
        // Arrange
        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
        {
            expected = expected != null ? DateTime.Parse((string)expected) : null;
        }

        // Act
        var result = TypeConversionHelper.ConvertTo(input, targetType);


        // Assert
        Assert.Equal(expected, result);


    }

    [Fact]
    public void ConvertTo_EmptyStringToNullable_ReturnsNull()
    {
        var result = TypeConversionHelper.ConvertTo("", typeof(int?));
        Assert.Null(result);
    }

    [Fact]
    public void ConvertTo_NullToNonNullable_ThrowsException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            TypeConversionHelper.ConvertTo(null, typeof(int)));
    }

    [Fact]
    public void ConvertTo_InvalidEnum_ThrowsException()
    {
        Assert.Throws<InvalidCastException>(() =>
            TypeConversionHelper.ConvertTo("InvalidValue", typeof(TestEnum)));
    }

    [Fact]
    public void ConvertTo_DecimalString_ReturnsDecimal()
    {
        var result = TypeConversionHelper.ConvertTo("123.45", typeof(decimal));
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void ConvertTo_DecimalString_ReturnsNullableDecimal()
    {
        var result = TypeConversionHelper.ConvertTo("123.45", typeof(decimal?));
        Assert.Equal((decimal?)123.45m, result);
    }


    public enum TestEnum
    {
        EnumValue,
        AnotherValue
    }
}
