using System.Text.RegularExpressions;
using Galaxon.Core.Exceptions;
using Galaxon.Core.Numbers;

namespace Galaxon.Quantities;

/// <summary>
/// Encapsulates a unit with an optional multiplier (coming from a prefix) and exponent.
/// </summary>
public class Unit
{
    public Unit(BaseUnit baseUnit, UnitPrefix? prefix = null, int exponent = 1)
    {
        BaseUnit = baseUnit;
        Prefix = prefix;
        Exponent = exponent;
    }

    public Unit(BaseUnit baseUnit, int exponent) : this(baseUnit, null, exponent)
    {
    }

    public BaseUnit BaseUnit { get; set; }

    public UnitPrefix? Prefix { get; set; }

    public int Exponent { get; set; }

    /// <summary>
    /// Automatically convert from a base unit to a unit as needed.
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <returns></returns>
    public static implicit operator Unit(BaseUnit baseUnit)
    {
        return new Unit(baseUnit);
    }

    /// <summary>
    /// Clone.
    /// We don't need to clone the Unit or Prefix objects as these are immutable.
    /// </summary>
    public Unit Clone()
    {
        return new Unit(BaseUnit, Prefix, Exponent);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Unit unit2)
        {
            return false;
        }
        return BaseUnit == unit2.BaseUnit && Prefix == unit2.Prefix && Exponent == unit2.Exponent;
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    #region String methods

    public string ToString(string format)
    {
        var strExp = Exponent == 1
            ? ""
            : (format == "U" ? Exponent.ToSuperscript() : Exponent.ToString());
        return $"{Prefix?.Symbol}{BaseUnit.Symbol}{strExp}";
    }

    public override string ToString()
    {
        return ToString("U");
    }

    /// <summary>
    /// Given a symbol string comprising a base unit plus possibly a prefix and/or exponent, return
    /// a Unit object that encapsulates this information.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static Unit Parse(string symbol)
    {
        var match = Regex.Match(symbol, $"^{Quantity.RxsPrefixBase}{Quantity.RxsUnitExp}$");
        if (!match.Success)
        {
            throw new ArgumentFormatException(nameof(symbol),
                "Incorrect format or invalid or unknown unit.");
        }

        var prefixBase = match.Groups["prefixBase"].Value;
        var strExp = match.Groups["exp"].Value;
        var exp = strExp == "" ? 1 : int.Parse(strExp);

        // Look for a valid match of prefix and base unit.
        foreach (var baseUnit in BaseUnit.AllKnown)
        {
            // Check base symbol with no prefix.
            if (baseUnit.Symbol == prefixBase)
            {
                return new Unit(baseUnit);
            }

            // Look for a match with a prefix.
            var prefix = baseUnit.ValidPrefixes?
                .FirstOrDefault(prefix => $"{prefix.Symbol}{baseUnit.Symbol}" == prefixBase);
            if (prefix != null)
            {
                return new Unit(baseUnit, prefix, exp);
            }
        }

        // No match found.
        throw new ArgumentFormatException(nameof(symbol),
            "Incorrect format or invalid or unknown unit.");
    }

    #endregion String methods
}
