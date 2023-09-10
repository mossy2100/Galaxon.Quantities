namespace Galaxon.Quantities;

public static class Density
{
    /// <summary>
    /// Convert a density given in g/cm3 to kg/m3.
    /// </summary>
    /// <param name="gramsPerCm3">Density in g/cm3.</param>
    /// <returns>Density in kg/m3.</returns>
    public static double GramsPerCm3ToKgPerM3(double gramsPerCm3)
    {
        return gramsPerCm3 * 1000;
    }
}
