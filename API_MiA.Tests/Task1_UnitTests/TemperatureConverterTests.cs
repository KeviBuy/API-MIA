using System;
using System.Text.RegularExpressions;
using Xunit;
using ЛР3_МіА._АПІшка.Services;

namespace ЛР3_МіА._АПІшка.Tests.Task1_UnitTests;

public class TemperatureConverterTests
{
    private readonly TemperatureConverter _converter = new();

    //1. Перевірка значень (Equality: Assert.Equal, Assert.NotEqual)

    [Fact]
    public void ToCelsius_FromCelsius_ReturnsSameValue()
    {
        // Arrange
        double inputCelsius = 25.0;

        // Act
        double result = _converter.ToCelsius(inputCelsius, TemperatureScale.Celsius);

        // Assert
        Assert.Equal(24.0, result);
        Assert.NotEqual(0.0, result);
    }

    [Theory]
    [InlineData(0.0, 32.0)]
    [InlineData(100.0, 212.0)]
    [InlineData(-40.0, -40.0)]
    public void ToFahrenheit_FromCelsius_CalculatesCorrectValue(double celsius, double expectedFahrenheit)
    {
        // Arrange & Act
        double actual = _converter.ToFahrenheit(celsius, TemperatureScale.Celsius);

        // Assert
        Assert.Equal(expectedFahrenheit, actual, precision: 2);
    }

    // 2. Булеві перевірки (Assert.True, Assert.False)

    [Theory]
    [InlineData(-5.0, true)]
    [InlineData(-0.1, true)]
    [InlineData(0.0, false)]
    [InlineData(15.5, false)]
    public void IsBelowFreezing_GivenCelsiusValue_ReturnsExpectedBoolean(double celsius, bool expected)
    {
        // Arrange & Act
        bool isFreezing = _converter.IsBelowFreezing(celsius);

        // Assert
        if (expected)
        {
            Assert.True(isFreezing, $"{celsius}°C має вважатися температурою замерзання");
        }
        else
        {
            Assert.False(isFreezing, $"{celsius}°C не повинна вважатися температурою замерзання");
        }
    }

    // 3. Перевірка винятків (Assert.Throws<T>)

    [Theory]
    [InlineData(-274.0, TemperatureScale.Celsius)]
    [InlineData(-460.0, TemperatureScale.Fahrenheit)]
    [InlineData(-1.0, TemperatureScale.Kelvin)]
    public void ConvertMethods_WhenBelowAbsoluteZero_ThrowsArgumentOutOfRangeException(double value, TemperatureScale scale)
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _converter.ToCelsius(value, scale));

        Assert.Contains("нижче абсолютного нуля", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // 4. Перевірка референсів (Assert.Same, Assert.NotSame, Assert.Null, Assert.NotNull)

    [Fact]
    public void CreateReading_ReturnsValidObjectReference()
    {
        // Arrange
        double temp = 22.5;

        // Act
        var reading1 = _converter.CreateReading(temp, TemperatureScale.Celsius);
        var reading2 = _converter.CreateReading(temp, TemperatureScale.Celsius);
        TemperatureReading? nullReading = null;

        // Assert
        Assert.NotNull(reading1);
        Assert.Null(nullReading);

        // Два окремих виклики створюють різні об'єкти в купі (Heap)
        Assert.NotSame(reading1, reading2);

        // Посилання на один і той же об'єкт
        var alias = reading1;
        Assert.Same(reading1, alias);
    }

    // 5. Перевірка рядків (Assert.Contains, Assert.StartsWith, Assert.Matches)

    [Fact]
    public void GetTemperatureDescription_ReturnsFormattedDescriptionString()
    {
        // Arrange
        double temp = 21.0;

        // Act
        string description = _converter.GetTemperatureDescription(temp);

        // Assert
        Assert.StartsWith("Status:", description);
        Assert.Contains("Comfortable", description);
        // Перевірка формату через регулярний вираз
        Assert.Matches(new Regex(@"^Status:\s+[A-Za-z\s]+$"), description);
    }

    // 6. Перевірка діапазонів (Assert.InRange, Assert.NotInRange)

    [Fact]
    public void ToKelvin_FromNormalEarthTemperatures_ResultFallsInRange()
    {
        // Arrange
        double comfortableCelsius = 20.0;

        // Act
        double kelvin = _converter.ToKelvin(comfortableCelsius, TemperatureScale.Celsius);

        // Assert: кімнатна температура в Кельвінах лежить у межах [290K, 300K]
        Assert.InRange(kelvin, 290.0, 300.0);
        // Не повинна лежати в зоні екстремального холоду
        Assert.NotInRange(kelvin, 0.0, 200.0);
    }

    // 7. Передача даних через [Theory] та [MemberData]

    [Theory]
    [MemberData(nameof(TemperatureConverterMemberData.KelvinToCelsiusData), MemberType = typeof(TemperatureConverterMemberData))]
    public void ToCelsius_FromKelvin_UsingMemberData_ReturnsExpectedResult(double kelvin, double expectedCelsius)
    {
        // Arrange & Act
        double actual = _converter.ToCelsius(kelvin, TemperatureScale.Kelvin);

        // Assert
        Assert.Equal(expectedCelsius, actual, precision: 2);
    }

    [Theory]
    [MemberData(nameof(TemperatureConverterMemberData.CategoryDescriptionsData), MemberType = typeof(TemperatureConverterMemberData))]
    public void GetTemperatureDescription_UsingMemberData_ContainsKeyword(double celsius, string expectedKeyword)
    {
        // Arrange & Act
        string desc = _converter.GetTemperatureDescription(celsius);

        // Assert
        Assert.Contains(expectedKeyword, desc);
    }

    // 8. Передача даних через [Theory] та [ClassData]

    [Theory]
    [ClassData(typeof(FahrenheitToCelsiusClassData))]
    public void ToCelsius_FromFahrenheit_UsingClassData_CalculatesCorrectly(double fahrenheit, double expectedCelsius)
    {
        // Arrange & Act
        double actual = _converter.ToCelsius(fahrenheit, TemperatureScale.Fahrenheit);

        // Assert
        Assert.Equal(expectedCelsius, actual, precision: 2);
    }
}
