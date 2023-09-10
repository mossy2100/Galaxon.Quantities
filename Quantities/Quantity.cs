using System.Text;
using System.Text.RegularExpressions;
using Galaxon.Core.Exceptions;
using Galaxon.Core.Numbers;
using Galaxon.Core.Strings;

namespace Galaxon.Quantities;

public class Quantity
{
    #region Cast operators

    /// <summary>
    /// Convert a double value into a Quantity.
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static implicit operator Quantity(double d)
    {
        return new Quantity(d);
    }

    #endregion Cast operators

    #region Constructors

    // /// <summary>
    // /// Default constructor.
    // /// </summary>
    public Quantity()
    {
    }

    /// <summary>
    /// Construct from a amount and a unit.
    /// </summary>
    public Quantity(double amount, Unit? unit = null)
    {
        Amount = amount;
        if (unit != null)
        {
            Units.Add(unit);
        }
    }

    /// <summary>
    /// Construct from a unit.
    /// </summary>
    public Quantity(Unit unit) : this(1, unit)
    {
    }

    /// <summary>
    /// Construct from a amount and a unit.
    /// </summary>
    public Quantity(double amount, BaseUnit baseUnit) : this(amount, new Unit(baseUnit))
    {
    }

    /// <summary>
    /// Construct from a unit.
    /// </summary>
    public Quantity(BaseUnit baseUnit) : this(1, baseUnit)
    {
    }

    /// <summary>
    /// Construct from a amount as double and units as string.
    /// Example:
    /// Quantity m = new(-12.34, "kg*m/s2");
    /// Quantity m = new(Math.PI, "rad");
    /// Quantity m = new(5, "ft");
    /// Quantity m = new(100, "$/h");
    /// </summary>
    public Quantity(double amount, string? units)
    {
        if (units == null)
        {
            Amount = amount;
        }
        else
        {
            // Parse the units string. Note, this could throw.
            var m = Parse(units);

            // So far so good, construct.
            Amount = amount * m.Amount;
            Units = m.Units;
        }
    }

    /// <summary>
    /// Construct from a string. If the amount isn't specified, it will default to 1.
    /// Example:
    /// Quantity m = new("kg*m/s2");
    /// Quantity m = new("1.2 rad");
    /// Quantity m = new("5 ft 10 in");
    /// </summary>
    public Quantity(string? units) : this(1, units)
    {
    }

    #endregion Constructors

    #region Properties

    public double Amount { get; set; } = 1;

    // Collection of units, each with a prefix multiplier and exponent (both defaulting to 1).
    public List<Unit> Units { get; set; } = new ();

    #endregion Properties

    #region Constants

    // Regular expression strings.
    internal const string RxsUnitExp = @"(?<unitExp>(-?\d+)?)";

    internal const string RxsPrefixBase = @"(?<prefixBase>[a-zΩ°""'″′]{1,6})";

    internal const string RxsUnit = $@"(?<unit>{RxsPrefixBase}{RxsUnitExp})";

    internal const string RxsOp = @"(?<op>[\*⋅/])";

    internal const string RxsOpUnit = $"({RxsOp}{RxsUnit})";

    internal const string RxsSign = "(?<sign>[-+]?)";

    internal const string RxsDecimal = @"(?<amt>\d+(\.\d+)?)";

    internal const string RxsCurrencySymbol = $"(?<currencySymbol>[{BaseUnit.CurrencySymbols}])";

    internal const string RxsCurrencyPrefix =
        $"(?<currencyPrefix>[{UnitPrefix.CurrencyPrefixes}]?)";

    internal const string RxsMoney =
        $@"({RxsSign}{RxsCurrencySymbol}{RxsDecimal}{RxsCurrencyPrefix}{RxsOpUnit}*)";

    internal const string RxsDouble = @"(?<amt>([-+]?\d+(\.\d+)?(e[-+]?\d+)?)?)";

    internal const string RxsCompoundUnit = $@"(?<compound>(?<op>/?){RxsUnit}{RxsOpUnit}*)";

    internal const string RxsPhysical = $"{RxsDouble} ?{RxsCompoundUnit}";

    #endregion Constants

    #region Miscellaneous methods

    /// <summary>
    /// Clone a quantity object.
    /// </summary>
    public Quantity Clone()
    {
        Quantity clone = new (Amount);
        foreach (var unit in Units)
        {
            clone.Units.Add(unit.Clone());
        }
        return clone;
    }

    /// <summary>
    /// Check if this quantity has the same units as another one.
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    public bool HasSameUnits(Quantity m)
    {
        var nUnits = Units.Count;

        // Check they have the same number of units.
        if (nUnits != m.Units.Count)
        {
            return false;
        }

        // Check they have the same units.
        return !Units.Where((t, i) => !t.Equals(m.Units[i])).Any();
    }

    public bool HasCompatibleUnits(Quantity m)
    {
        var m1 = Reduce();
        var m2 = m.Reduce();
        return m1.HasSameUnits(m2);
    }

    /// <summary>
    /// Tidy up the units.
    /// Because ToString() currently uses divide slashes instead of negative exponents, I'm putting
    /// units with positive exponents first. This is not the standard SI format but I think it's
    /// more common.
    /// </summary>
    private void Tidy()
    {
        Units = Units
            .Where(unit => unit.Exponent != 0)
            .OrderBy(unit => unit.Exponent > 0)
            .ThenBy(unit => +unit.BaseUnit.Order)
            .ToList();
    }

    /// <summary>
    /// Check if the quantity can be reduced to more basic units.
    /// </summary>
    /// <returns></returns>
    private bool CanBeReduced()
    {
        return Units.Any(unit => unit.BaseUnit.MetricUnitSymbol != null || unit.Prefix != null);
    }

    /// <summary>
    /// Reduce a quantity to the most basic units possible.
    /// These will either be SI base units or some other irreducible units like rad or p.
    /// This method removes all prefixes except for "k" (kilo) with "g" (grams), because kg is the
    /// SI base unit.
    /// </summary>
    public Quantity Reduce()
    {
        // Start with a copy.
        var result = Clone();

        // Continue until we have the most basic units.
        while (result.CanBeReduced())
        {
            // Construct a new set of units.
            List<Unit> newUnits = new ();

            // Convert to metric any units that can be.
            foreach (var unit in result.Units)
            {
                if (unit.BaseUnit.MetricUnitSymbol != null)
                {
                    // Use the ToMetric method if present.
                    // It's only valid if there are no other units and no exponents or prefixes.
                    // i.e. we wouldn't convert /°C (per degree celsius) to /K using this method.
                    // In those case the default multiplication method should be used.
                    Quantity metricQty;
                    if (unit.BaseUnit.ToMetric != null && result.Units.Count == 1
                        && unit.Prefix == null && unit.Exponent == 1)
                    {
                        // Calculate the new amount using the method.
                        result.Amount = unit.BaseUnit.ToMetric(unit.Prefix?.Multiplier ?? 1);

                        // Get the metric unit as a Quantity. This could throw.
                        metricQty = new Quantity(1, unit.BaseUnit.MetricUnitSymbol);
                    }
                    else
                    {
                        // Get the amount of the metric unit.
                        var amt = (unit.Prefix?.Multiplier ?? 1) *
                            (unit.BaseUnit.MetricUnitAmount ?? 1);

                        // Get the metric unit as a quantity, including the prefix. This could throw.
                        metricQty = new Quantity(amt, unit.BaseUnit.MetricUnitSymbol);

                        // Exponentiate if needed.
                        if (unit.Exponent != 1)
                        {
                            metricQty ^= unit.Exponent;
                        }

                        // Multiply the amount.
                        result.Amount *= metricQty.Amount;
                    }

                    // Add the metric units.
                    newUnits.AddRange(metricQty.Units);
                }
                else if (unit.Prefix != null)
                {
                    // Remove any prefixes (other than kilo for grams) through multiplication.
                    result.Amount *= Math.Pow(unit.Prefix?.Multiplier ?? 1, unit.Exponent);
                    newUnits.Add(new Unit(unit.BaseUnit, unit.Exponent));
                }
                else
                {
                    // No change to this unit needed.
                    newUnits.Add(unit);
                }
            } // foreach unit

            result.Units = newUnits;
            result.Tidy();
        } // while

        // Convert grams to kilograms to be consistent with SI.
        var grams = result.Units.FirstOrDefault(unit => unit.BaseUnit.Symbol == "g");
        if (grams != null)
        {
            result.Amount /= Math.Pow(1000, grams.Exponent);
            grams.Prefix = UnitPrefix.Get("k");
        }

        return result;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Quantity m)
        {
            return false;
        }
        return Amount.FuzzyEquals(m.Amount) && HasSameUnits(m);
    }

    public override int GetHashCode()
    {
        return ToAsciiString().GetHashCode();
    }

    public Quantity Convert(string? toUnit)
    {
        // Check if the provided unit is compatible.
        Quantity n = new (toUnit);
        if (!HasCompatibleUnits(n))
        {
            throw new ArgumentInvalidException(nameof(toUnit),
                $"The provided unit '{toUnit}' is incompatible.");
        }

        // TODO convert this quantity to toUnit.
        // Sort of a reverse of Reduce().
        return new Quantity();
    }

    public static double? ConvertAmount(double? amount, string fromUnit, string toUnit)
    {
        // Convert null to null and 0 to 0.
        if (amount is null or 0)
        {
            return amount;
        }

        Quantity m1 = new (amount.Value, fromUnit);
        var m2 = m1.Convert(toUnit);
        return m2.Amount;
    }

    #endregion Miscellaneous methods

    #region String methods

    /// <summary>
    /// Construct a quantity from a string.
    /// We'll accept the following formats:
    /// 1. A unit or compound unit by itself, e.g. "m", "kg*m/s2"
    /// 2. A unit and amount, optionally separated by a space. e.g. "100m", "3 ft".
    /// 3. A series of units and quantities, e.g. "5 ft 10 in", "3° 20′ 4″"
    /// Our goal is to extract an array of quantities and units.
    /// Example:
    /// Quantity m = Quantity.Parse("100km/h");
    /// </summary>
    /// <param name="strQuantity"></param>
    public static Quantity Parse(string strQuantity)
    {
        if (string.IsNullOrWhiteSpace(strQuantity))
        {
            throw new ArgumentFormatException("Invalid unit format.");
        }

        strQuantity = strQuantity.Trim();
        Unit? unit;

        // Check for a valid monetary string. These need to be handled differently because currency
        // units can come before quantities, and the prefix can be by itself after the amount.
        // Any numbers following a currency symbol are treated as part of the amount, not an
        // exponent, as with other units.
        var match = Regex.Match(strQuantity, $"^{RxsMoney}$", RegexOptions.IgnoreCase);
        var currencyMatch = match.Success;
        var physicalMatch = false;
        if (!currencyMatch)
        {
            // Check for valid physical amount string.
            match = Regex.Match(strQuantity, $"^{RxsPhysical}$", RegexOptions.IgnoreCase);
            physicalMatch = match.Success;
        }

        // If the input string matches neither, it's invalid.
        if (!currencyMatch && !physicalMatch)
        {
            throw new ArgumentFormatException(nameof(strQuantity),
                $"The string '{strQuantity}' is not in the correct format.");
        }

        // Get the amount.
        var strAmt = match.Groups["amt"].Value;
        var amt = strAmt == "" ? 1 : double.Parse(strAmt);

        if (currencyMatch)
        {
            // Get the sign, which in money values comes before the symbol.
            var strSign = match.Groups["sign"].Value;
            if (strSign == "-")
            {
                amt = -amt;
            }
        }

        // Create the result object.
        Quantity result = new (amt);

        if (currencyMatch)
        {
            // Get the currency symbol.
            var strCurrencySymbol = match.Groups["currencySymbol"].Value;
            unit = Unit.Parse(strCurrencySymbol);
            if (unit == null)
            {
                throw new ArgumentFormatException(
                    $"Currency unit '{strCurrencySymbol}' is invalid or unknown.");
            }

            // Get the currency prefix.
            var strCurrencyPrefix = match.Groups["currencyPrefix"].Value;
            if (strCurrencyPrefix != "")
            {
                var prefix = UnitPrefix.Get(strCurrencyPrefix);
                if (prefix == null)
                {
                    throw new ArgumentFormatException(nameof(strQuantity),
                        $"Invalid currency prefix '{strCurrencyPrefix}'.");
                }
                unit.Prefix = prefix;
            }

            // Add the currency unit to the result.
            result.Units.Add(unit);
        }

        // Process other units.
        var nUnits = match.Groups["unit"].Captures.Count;
        for (var i = 0; i < nUnits; i++)
        {
            // Look up the base unit.
            var strBaseUnit = match.Groups["prefixBase"].Captures[i].Value;
            unit = Unit.Parse(strBaseUnit);
            if (unit == null)
            {
                throw new ArgumentFormatException(
                    $"Base unit '{strBaseUnit}' is invalid or unknown.");
            }

            // Get the unit exponent. If the unit was preceded by a division operator, negate the
            // exponent.
            var strOp = match.Groups["op"].Captures[i].Value;
            var strUnitExp = match.Groups["unitExp"].Captures[i].Value;
            var unitExp = strUnitExp == "" ? 1 : int.Parse(strUnitExp);
            unit.Exponent = strOp == "/" ? -unitExp : unitExp;

            // Add the unit to the result.
            result.Units.Add(unit);
        }

        return result;
    }

    public string ToString(bool includeAmount, bool includeUnits, bool niceFormat = true)
    {
        if (!includeAmount && !includeUnits)
        {
            throw new ArgumentInvalidException(nameof(includeAmount),
                "Either the amount or the units or both must be included in the output string.");
        }

        StringBuilder sbResult = new ();

        if (includeAmount)
        {
            // Convert the amount to a string.
            // TODO Use built-in support for formatting currency values (format specifier "C").
            // <see href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings" />
            // Format "g" will be the more compact of "e" (exponential) or "f" (fixed point).
            var strAmount = Amount.ToString("g");

            if (niceFormat)
            {
                // Parse the amount string so we can extract the exponent and reformat using
                // superscript characters.
                const string rx = @"(?<num>-?\d+(\.\d+)?)(e(?<exp>[-+]?\d+))?";
                var match = Regex.Match(strAmount, rx, RegexOptions.IgnoreCase);

                // Format the number (without the exponent) in a nice way.
                var strNum = match.Groups["num"].Value;
                strNum = double.Parse(strNum).ToString("N");

                // Format the exponent in a nice way.
                var strExp = match.Groups["exp"].Value;
                if (strExp != "")
                {
                    strExp = $"×10{int.Parse(strExp).ToSuperscript()}";
                }

                strAmount = $"{strNum}{strExp}";
            }

            sbResult.Append(strAmount);
        }

        // Check if we just want to return the amount.
        if (!includeUnits || Units.Count == 0)
        {
            return sbResult.ToString();
        }

        StringBuilder sbUnits = new ();

        // Append the units. They should already be tidy.
        for (var i = 0; i < Units.Count; i++)
        {
            var unit = Units[i];
            string strOperator;
            string strUnit;
            var strSymbol = unit.BaseUnit.Symbol;
            if (unit.Exponent < 0)
            {
                strOperator = "/";
                strUnit =
                    new Unit(unit.BaseUnit, unit.Prefix, -unit.Exponent).ToString(niceFormat
                        ? "U"
                        : "A");
                sbUnits.Append($"{strOperator}{strUnit}");
            }
            else
            {
                // Check for a currency unit with exponent of 1.
                if (BaseUnit.CurrencySymbols.Contains(strSymbol) && unit.Exponent == 1)
                {
                    // Put the currency symbol before the amount and the prefix (if any) after.
                    // e.g.
                    // TODO Use "c" format specifier to do this.
                    sbResult.Prepend(strSymbol);
                    sbResult.Append(unit.Prefix);
                }
                else
                {
                    // For units with positive exponents, put a space before the first one, a dot
                    // before the others.
                    // The exception is we don't want spaces before degree, arcminute, or arcsecond
                    // symbols.
                    strOperator = i == 0
                        ? strSymbol is "°" or "′" or "″" ? "" : " "
                        : "⋅";
                    strUnit = unit.ToString(niceFormat ? "U" : "A");
                    sbUnits.Append($"{strOperator}{strUnit}");
                }
            }
        }

        sbResult.Append(sbUnits);
        return sbResult.ToString();
    }

    /// <summary>
    /// Convert to string with special characters, suitable for documents.
    /// </summary>
    public override string ToString()
    {
        return ToString(true, true);
    }

    /// <summary>
    /// Convert to string with only boring old ASCII characters.
    /// </summary>
    public string ToAsciiString()
    {
        return ToString(true, true, false);
    }

    /// <summary>
    /// Get just the amount as a string.
    /// </summary>
    public string ToAmountString()
    {
        return ToString(true, false);
    }

    /// <summary>
    /// Get just the units as a string.
    /// </summary>
    public string ToUnitsString()
    {
        return ToString(false, true);
    }

    #endregion String methods

    #region Arithmetic methods

    /// <summary>
    /// Multiply 2 quantities.
    /// </summary>
    /// <param name="qty1"></param>
    /// <param name="qty2"></param>
    /// <returns></returns>
    public static Quantity Multiply(Quantity qty1, Quantity qty2)
    {
        // Normalize the two operands. Reduce() will create a new Quantity object.
        var result = qty1.Reduce();
        var qty2Metric = qty2.Reduce();

        // Get the result amount.
        result.Amount *= qty2Metric.Amount;

        // Multiply m1 by each unit in m2.
        foreach (var unit2 in qty2Metric.Units)
        {
            var unit1 = result.Units.FirstOrDefault(unit => unit.BaseUnit == unit2.BaseUnit);
            if (unit1 == null)
            {
                result.Units.Add(unit2);
            }
            else
            {
                unit1.Exponent += unit2.Exponent;
            }
        }

        result.Tidy();
        return result;
    }

    /// <summary>
    /// Get the inverse of a quantity.
    /// </summary>
    public static Quantity Inverse(Quantity qty)
    {
        var result = qty.Clone();

        // Inverse the amount.
        result.Amount = 1 / result.Amount;

        // Negate the exponents.
        foreach (var unit in result.Units)
        {
            unit.Exponent = -unit.Exponent;
        }

        return result;
    }

    /// <summary>
    /// Divide one quantity by another quantity.
    /// </summary>
    /// <param name="qty1"></param>
    /// <param name="qty2"></param>
    public static Quantity Divide(Quantity qty1, Quantity qty2)
    {
        return Multiply(qty1, Inverse(qty2));
    }

    /// <summary>
    /// Add 2 quantities.
    /// </summary>
    /// <param name="qty1"></param>
    /// <param name="qty2"></param>
    /// <returns></returns>
    public static Quantity Add(Quantity qty1, Quantity qty2)
    {
        var result = qty1.Reduce();
        var qty2Metric = qty2.Reduce();

        if (!result.HasSameUnits(qty2Metric))
        {
            throw new ArgumentInvalidException(nameof(qty1),
                "Cannot add two quantities with incompatible units.");
        }

        result.Amount += qty2Metric.Amount;
        return result;
    }

    /// <summary>
    /// Negate a quantity.
    /// </summary>
    /// <returns></returns>
    public static Quantity Negate(Quantity qty)
    {
        var result = qty.Clone();
        result.Amount = -result.Amount;
        return result;
    }

    /// <summary>
    /// Subtract one quantity from another.
    /// </summary>
    /// <param name="qty1"></param>
    /// <param name="qty2"></param>
    /// <returns></returns>
    public static Quantity Subtract(Quantity qty1, Quantity qty2)
    {
        return Add(qty1, Negate(qty2));
    }

    /// <summary>
    /// Raise the quantity to a given power.
    /// Only integer exponents supported at this time.
    /// </summary>
    /// <param name="qty"></param>
    /// <param name="exp"></param>
    public static Quantity Pow(Quantity qty, int exp)
    {
        // Anything to the power of 0 is 1.
        if (exp == 0)
        {
            return 1;
        }

        var result = qty.Clone();

        // Anything to the power of 1 is itself.
        if (exp == 1)
        {
            return result;
        }

        // Exponentiate the amount.
        result.Amount = Math.Pow(result.Amount, exp);

        // Multiply the unit exponents.
        foreach (var unit in result.Units)
        {
            unit.Exponent *= exp;
        }

        return result;
    }

    #endregion Arithmetic methods

    #region Operators

    /// <summary>
    /// Multiplication operator.
    /// </summary>
    /// <param name="qty1"></param>
    /// <param name="qty2"></param>
    /// <returns></returns>
    public static Quantity operator *(Quantity qty1, Quantity qty2)
    {
        return Multiply(qty1, qty2);
    }

    /// <summary>
    /// Division operator.
    /// </summary>
    /// <param name="qty1"></param>
    /// <param name="qty2"></param>
    /// <returns></returns>
    public static Quantity operator /(Quantity qty1, Quantity qty2)
    {
        return Divide(qty1, qty2);
    }

    /// <summary>
    /// Addition operator.
    /// </summary>
    /// <param name="qty1"></param>
    /// <param name="qty2"></param>
    /// <returns></returns>
    public static Quantity operator +(Quantity qty1, Quantity qty2)
    {
        return Add(qty1, qty2);
    }

    /// <summary>
    /// Negation operator (unary minus).
    /// </summary>
    /// <param name="qty1"></param>
    /// <returns></returns>
    public static Quantity operator -(Quantity qty1)
    {
        return Negate(qty1);
    }

    /// <summary>
    /// Subtraction operator.
    /// </summary>
    /// <param name="qty1"></param>
    /// <param name="qty2"></param>
    /// <returns></returns>
    public static Quantity operator -(Quantity qty1, Quantity qty2)
    {
        return Subtract(qty1, qty2);
    }

    /// <summary>
    /// Exponentiation operator. Only integer exponents supported.
    /// </summary>
    /// <param name="qty"></param>
    /// <param name="exp"></param>
    /// <returns></returns>
    public static Quantity operator ^(Quantity qty, int exp)
    {
        return Pow(qty, exp);
    }

    #endregion Operators
}
