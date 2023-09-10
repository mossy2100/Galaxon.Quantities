using Galaxon.Core.Numbers;

namespace Galaxon.Quantities.Tests;

[TestClass]
public class TestTemperature
{
    private static readonly List<(double k, double c, double f)> _EquivalentTemps = new ()
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
        foreach (var set in _EquivalentTemps)
        {
            Assert.AreEqual(set.k, Temperature.CelsiusToKelvin(set.c), XDouble.Delta);
        }
    }

    [TestMethod]
    public void TestKelvinToCelsius()
    {
        foreach (var set in _EquivalentTemps)
        {
            Assert.AreEqual(set.c, Temperature.KelvinToCelsius(set.k), XDouble.Delta);
        }
    }

    [TestMethod]
    public void TestCelsiusToFahrenheit()
    {
        foreach (var set in _EquivalentTemps)
        {
            Assert.AreEqual(set.f, Temperature.CelsiusToFahrenheit(set.c), XDouble.Delta);
        }
    }

    [TestMethod]
    public void TestFahrenheitToCelsius()
    {
        foreach (var set in _EquivalentTemps)
        {
            Assert.AreEqual(set.c, Temperature.FahrenheitToCelsius(set.f), XDouble.Delta);
        }
    }

    [TestMethod]
    public void TestFahrenheitToKelvin()
    {
        foreach (var set in _EquivalentTemps)
        {
            Assert.AreEqual(set.k, Temperature.FahrenheitToKelvin(set.f), XDouble.Delta);
        }
    }

    [TestMethod]
    public void TestKelvinToFahrenheit()
    {
        foreach (var set in _EquivalentTemps)
        {
            Assert.AreEqual(set.f, Temperature.KelvinToFahrenheit(set.k), XDouble.Delta);
        }
    }
}
