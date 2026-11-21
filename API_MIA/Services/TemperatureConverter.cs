namespace ЛР3_МіА._АПІшка.Services;

public enum TemperatureScale
{
    Celsius,
    Fahrenheit,
    Kelvin
}

public class TemperatureReading
{
    public double Value { get; set; }
    public TemperatureScale Scale { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TemperatureConverter
{
    public const double AbsoluteZeroCelsius = -273.15;
    public const double AbsoluteZeroFahrenheit = -459.67;
    public const double AbsoluteZeroKelvin = 0.0;

    public double ToCelsius(double value, TemperatureScale fromScale)
    {
        ValidateTemperature(value, fromScale);

        return fromScale switch
        {
            TemperatureScale.Celsius => value,
            TemperatureScale.Fahrenheit => Math.Round((value - 32.0) * 5.0 / 9.0, 2),
            TemperatureScale.Kelvin => Math.Round(value - 273.15, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(fromScale), "Невідома шкала температур")
        };
    }

    public double ToFahrenheit(double value, TemperatureScale fromScale)
    {
        ValidateTemperature(value, fromScale);

        return fromScale switch
        {
            TemperatureScale.Fahrenheit => value,
            TemperatureScale.Celsius => Math.Round(value * 9.0 / 5.0 + 32.0, 2),
            TemperatureScale.Kelvin => Math.Round((value - 273.15) * 9.0 / 5.0 + 32.0, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(fromScale), "Невідома шкала температур")
        };
    }

    public double ToKelvin(double value, TemperatureScale fromScale)
    {
        ValidateTemperature(value, fromScale);

        return fromScale switch
        {
            TemperatureScale.Kelvin => value,
            TemperatureScale.Celsius => Math.Round(value + 273.15, 2),
            TemperatureScale.Fahrenheit => Math.Round((value - 32.0) * 5.0 / 9.0 + 273.15, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(fromScale), "Невідома шкала температур")
        };
    }

    public bool IsBelowFreezing(double celsius)
    {
        ValidateTemperature(celsius, TemperatureScale.Celsius);
        return celsius < 0.0;
    }

    public string GetTemperatureDescription(double celsius)
    {
        ValidateTemperature(celsius, TemperatureScale.Celsius);

        if (celsius <= AbsoluteZeroCelsius) return "Status: Absolute Zero (-273.15°C)";
        if (celsius < 0.0) return "Status: Freezing cold temperature";
        if (celsius <= 15.0) return "Status: Cool temperature";
        if (celsius <= 25.0) return "Status: Comfortable room temperature";
        if (celsius <= 35.0) return "Status: Warm summer temperature";
        return "Status: Extreme hot temperature";
    }

    public TemperatureReading CreateReading(double value, TemperatureScale scale)
    {
        ValidateTemperature(value, scale);
        double inCelsius = ToCelsius(value, scale);

        return new TemperatureReading
        {
            Value = value,
            Scale = scale,
            Description = GetTemperatureDescription(inCelsius)
        };
    }

    private void ValidateTemperature(double value, TemperatureScale scale)
    {
        switch (scale)
        {
            case TemperatureScale.Celsius when value < AbsoluteZeroCelsius:
                throw new ArgumentOutOfRangeException(nameof(value), $"Температура {value}°C нижче абсолютного нуля (-273.15°C).");
            case TemperatureScale.Fahrenheit when value < AbsoluteZeroFahrenheit:
                throw new ArgumentOutOfRangeException(nameof(value), $"Температура {value}°F нижче абсолютного нуля (-459.67°F).");
            case TemperatureScale.Kelvin when value < AbsoluteZeroKelvin:
                throw new ArgumentOutOfRangeException(nameof(value), $"Температура {value}K нижче абсолютного нуля (0K).");
        }
    }
}
