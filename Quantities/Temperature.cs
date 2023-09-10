namespace Galaxon.Quantities;

public static class Temperature
{
    /// <summary>
    /// Number of degrees difference between 0K and 0°C.
    /// </summary>
    public const double CelsiusKelvinDiff = 273.15;

    /// <summary>
    /// Number of degrees difference between 0°F and 0°C.
    /// </summary>
    public const double CelsiusFahrenheitDiff = 32;

    /// <summary>
    /// Number of degrees Celsius (or kelvins) per degree of Fahrenheit.
    /// </summary>
    public const double CelsiusPerFahrenheit = 5.0 / 9;

    /// <summary>
    /// Convert a temperature in Celsius to Kelvin.
    /// </summary>
    /// <param name="c">Temperature in Celsius.</param>
    /// <returns>Temperature in Kelvin.</returns>
    public static double CelsiusToKelvin(double c)
    {
        return c + CelsiusKelvinDiff;
    }

    public static double KelvinToCelsius(double k)
    {
        return k - CelsiusKelvinDiff;
    }

    public static double CelsiusToFahrenheit(double c)
    {
        return c / CelsiusPerFahrenheit + CelsiusFahrenheitDiff;
    }

    public static double FahrenheitToCelsius(double f)
    {
        return (f - CelsiusFahrenheitDiff) * CelsiusPerFahrenheit;
    }

    public static double FahrenheitToKelvin(double f)
    {
        return CelsiusToKelvin(FahrenheitToCelsius(f));
    }

    public static double KelvinToFahrenheit(double k)
    {
        return CelsiusToFahrenheit(KelvinToCelsius(k));
    }
}
