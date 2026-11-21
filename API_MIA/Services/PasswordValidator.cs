using System;
using System.Collections.Generic;
using System.Linq;

namespace ЛР3_МіА._АПІшка.Services;

public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public int Score { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class PasswordValidator
{
    private static readonly HashSet<char> SpecialCharacters = new("!@#$%^&*()_+-=[]{}|;':\",./<>?`~");

    // Цикл 1 TDD: Перевірка довжини пароля (Length Validation)

    public bool ValidateLength(string? password, int minLength = 8, int maxLength = 64)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password), "Пароль не може бути null.");

        if (minLength > maxLength)
            throw new ArgumentException("Мінімальна довжина не може перевищувати максимальну.");

        return password.Length >= minLength && password.Length <= maxLength;
    }

    // 2. Перевірка категорій символів (Character Categories)

    public bool HasUpperCase(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return password.Any(char.IsUpper);
    }

    public bool HasLowerCase(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return password.Any(char.IsLower);
    }

    public bool HasDigit(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return password.Any(char.IsDigit);
    }

    public bool HasSpecialCharacter(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return password.Any(ch => SpecialCharacters.Contains(ch) || (!char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch)));
    }

    // 3. Комплексна валідація складності (Full Password Validation & Scoring)

    public PasswordValidationResult Validate(string? password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password), "Пароль не може бути null.");

        var result = new PasswordValidationResult();
        int score = 0;

        if (!ValidateLength(password, 8, 64))
        {
            result.Errors.Add("Пароль повинен мати довжину від 8 до 64 символів.");
        }
        else
        {
            score += 20;
            if (password.Length >= 12) score += 15;
            if (password.Length >= 16) score += 10;
        }

        if (!HasUpperCase(password))
        {
            result.Errors.Add("Пароль повинен містити хоча б одну велику літеру.");
        }
        else
        {
            score += 15;
        }

        if (!HasLowerCase(password))
        {
            result.Errors.Add("Пароль повинен містити хоча б одну малу літеру.");
        }
        else
        {
            score += 10;
        }

        if (!HasDigit(password))
        {
            result.Errors.Add("Пароль повинен містити хоча б одну цифру.");
        }
        else
        {
            score += 15;
        }

        if (!HasSpecialCharacter(password))
        {
            result.Errors.Add("Пароль повинен містити хоча б один спеціальний символ.");
        }
        else
        {
            score += 15;
        }

        result.Score = Math.Min(100, score);
        result.IsValid = result.Errors.Count == 0;

        return result;
    }
}
