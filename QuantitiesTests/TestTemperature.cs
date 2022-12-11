using AstroMultimedia.Core.Testing;

namespace AstroMultimedia.Quantities.Tests;

[TestClass]
public class TestTemperature
{
    private static readonly List<(double k, double c, double f)> s_equivalentTemps = new()
    {
        // Absolute zero.
        (0, -273.15, -459.67),
        // Lowest temperature ever recorded on Earth.
        (183.75, -89.4, -128.92),
        // Temperature where Celsius and Fahrenheit are equal.
        (233.15, -40, -40),
        // Freezing point of water.
        (273.15, 0, 32),
        // Standard laboratory conditions.
        (298.15, 25, 77),
        // Highest temperature ever recorded on Earth.
        (329.85, 56.7, 134.06),
        // Boiling point of water.
        (373.15, 100, 212)
    };

    [TestMethod]
    public void TestCelsiusToKelvin()
    {
        foreach ((double k, double c, double f) set in s_equivalentTemps)
        {
            XAssert.AreEqual(set.k, Temperature.CelsiusToKelvin(set.c));
        }
    }

    [TestMethod]
    public void TestKelvinToCelsius()
    {
        foreach ((double k, double c, double f) set in s_equivalentTemps)
        {
            XAssert.AreEqual(set.c, Temperature.KelvinToCelsius(set.k));
        }
    }

    [TestMethod]
    public void TestCelsiusToFahrenheit()
    {
        foreach ((double k, double c, double f) set in s_equivalentTemps)
        {
            XAssert.AreEqual(set.f, Temperature.CelsiusToFahrenheit(set.c));
        }
    }

    [TestMethod]
    public void TestFahrenheitToCelsius()
    {
        foreach ((double k, double c, double f) set in s_equivalentTemps)
        {
            XAssert.AreEqual(set.c, Temperature.FahrenheitToCelsius(set.f));
        }
    }

    [TestMethod]
    public void TestFahrenheitToKelvin()
    {
        foreach ((double k, double c, double f) set in s_equivalentTemps)
        {
            XAssert.AreEqual(set.k, Temperature.FahrenheitToKelvin(set.f));
        }
    }

    [TestMethod]
    public void TestKelvinToFahrenheit()
    {
        foreach ((double k, double c, double f) set in s_equivalentTemps)
        {
            XAssert.AreEqual(set.f, Temperature.KelvinToFahrenheit(set.k));
        }
    }
}
