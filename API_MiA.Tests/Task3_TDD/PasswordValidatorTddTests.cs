using System;
using FluentAssertions;
using Xunit;
using ЛР3_МіА._АПІшка.Services;

namespace ЛР3_МіА._АПІшка.Tests.Task3_TDD;

public class PasswordValidatorTddTests
{
    private readonly PasswordValidator _validator = new();

    //ЦИКЛ 1 TDD: Перевірка довжини пароля

    [Fact]
    public void Cycle1_ValidateLength_WhenPasswordIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullPassword = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _validator.ValidateLength(nullPassword));
    }

    [Theory]
    [InlineData("1234567", 8, 64, false)]
    [InlineData("Pass1234", 8, 64, true)]
    [InlineData("VeryLongValidPassword12345", 8, 64, true)]
    public void Cycle1_ValidateLength_GivenVariousPasswords_ReturnsExpectedResult(
        string password, int min, int max, bool expected)
    {
        // Act
        bool result = _validator.ValidateLength(password, min, max);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Cycle1_ValidateLength_WhenMinLengthGreaterThanMaxLength_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _validator.ValidateLength("password", minLength: 20, maxLength: 10));
    }

    // ЦИКЛ 2 TDD: Перевірка категорій символів

    [Theory]
    [InlineData("password", false)]
    [InlineData("Password", true)]
    [InlineData("PASSWORD", true)]
    public void Cycle2_HasUpperCase_ChecksPresenceOfUppercaseLetters(string password, bool expected)
    {
        // Act
        bool result = _validator.HasUpperCase(password);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("PASSWORD", false)]
    [InlineData("Password", true)]
    [InlineData("password", true)]
    public void Cycle2_HasLowerCase_ChecksPresenceOfLowercaseLetters(string password, bool expected)
    {
        // Act
        bool result = _validator.HasLowerCase(password);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Password", false)]
    [InlineData("Password123", true)]
    public void Cycle2_HasDigit_ChecksPresenceOfDigits(string password, bool expected)
    {
        // Act
        bool result = _validator.HasDigit(password);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Password123", false)]
    [InlineData("Password123!", true)]
    [InlineData("P@ssw0rd#", true)]
    public void Cycle2_HasSpecialCharacter_ChecksPresenceOfSymbols(string password, bool expected)
    {
        // Act
        bool result = _validator.HasSpecialCharacter(password);

        // Assert
        result.Should().Be(expected);
    }

    // ЦИКЛ 3 TDD: Комплексна валідація складності

    [Fact]
    public void Cycle3_Validate_WhenPasswordIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _validator.Validate(null));
    }

    [Fact]
    public void Cycle3_Validate_WhenPasswordMeetsAllCriteria_ReturnsValidWithHighScore()
    {
        // Arrange
        string strongPassword = "StrongP@ssw0rd2026!";

        // Act
        var result = _validator.Validate(strongPassword);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Score.Should().BeGreaterThanOrEqualTo(80);
    }

    [Fact]
    public void Cycle3_Validate_WhenPasswordIsWeak_ReturnsInvalidWithDetailedErrors()
    {
        // Arrange
        string weakPassword = "pass";

        // Act
        var result = _validator.Validate(weakPassword);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("довжин"));
        result.Errors.Should().Contain(e => e.Contains("велик"));
        result.Errors.Should().Contain(e => e.Contains("цифр"));
        result.Errors.Should().Contain(e => e.Contains("спеціальн"));
        result.Score.Should().BeLessThan(50);
    }
}
