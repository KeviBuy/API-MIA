using System.Collections;
using System.Collections.Generic;
using ЛР3_МіА._АПІшка.Services;

namespace ЛР3_МіА._АПІшка.Tests.Task1_UnitTests;
public class FahrenheitToCelsiusClassData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 32.0, 0.0 };       // Точка замерзання води
        yield return new object[] { 212.0, 100.0 };   // Точка кипіння води
        yield return new object[] { 98.6, 37.0 };     // Нормальна температура тіла
        yield return new object[] { -40.0, -40.0 };   // Спільна точка шкал
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
public static class TemperatureConverterMemberData
{
    public static IEnumerable<object[]> KelvinToCelsiusData =>
        new List<object[]>
        {
            new object[] { 0.0, -273.15 },     // Абсолютний нуль
            new object[] { 273.15, 0.0 },      // 0 °C
            new object[] { 373.15, 100.0 },    // 100 °C
            new object[] { 310.15, 37.0 }      // 37 °C
        };

    public static IEnumerable<object[]> CategoryDescriptionsData =>
        new List<object[]>
        {
            new object[] { -273.15, "Absolute Zero" },
            new object[] { -10.0, "Freezing" },
            new object[] { 20.0, "Comfortable" },
            new object[] { 40.0, "Extreme hot" }
        };
}
