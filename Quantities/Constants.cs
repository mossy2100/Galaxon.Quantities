namespace Galaxon.Quantities;

/// <summary>
///     <see href="https://en.wikipedia.org/wiki/Physical_constant#Table_of_physical_constants" />
/// </summary>
public static class Constants
{
    /// <summary>
    /// Elementary charge.
    /// </summary>
    public static readonly Quantity ElementaryCharge = new (1.602_176_634e-19, "C");

    /// <summary>
    /// Newtonian constant of gravitation.
    /// </summary>
    public static readonly Quantity Gravitation = new (6.67430e-11, "m3/kg/s2");

    /// <summary>
    /// Planck constant.
    /// </summary>
    public static readonly Quantity Planck = new (6.626_070_15e-34, "J*s");

    /// <summary>
    /// Reduced Planck constant or Dirac constant.
    /// </summary>
    public static readonly Quantity Dirac = Planck / Tau;

    /// <summary>
    /// Speed of light.
    /// </summary>
    public static readonly Quantity SpeedOfLight = new (299_792_458, "m/s");

    /// <summary>
    /// Vacuum electric permittivity.
    /// </summary>
    public static readonly Quantity ElectricPermittivity = new (8.854_187_8128e-12, "F/m");

    /// <summary>
    /// Vacuum magnetic permeability.
    /// </summary>
    public static readonly Quantity MagneticPermeability = new (1.256_637_062_12e-6, "N/A2");

    /// <summary>
    /// Electron mass.
    /// </summary>
    public static readonly Quantity ElectronMass = new (9.109_383_7015e-31, "kg");

    /// <summary>
    /// Fine structure constant.
    /// </summary>
    public static readonly Quantity FineStructure = new (7.297_352_5693e-3);

    /// <summary>
    /// Josephson constant.
    /// </summary>
    public static readonly Quantity Josephson = new (483_597.848_416_98e9, "Hz/V");

    /// <summary>
    /// Rydberg constant.
    /// </summary>
    public static readonly Quantity Rydberg = new (10_973_731.568_160, "m-1");

    /// <summary>
    /// Von Klitzing constant.
    /// </summary>
    public static readonly Quantity VonKlitzing = new (25_812.807_45, "Ω");

    /// <summary>
    /// Avogadro constant.
    /// </summary>
    public static readonly Quantity Avogadro = new (6.02214076E23, "mol-1");
}
