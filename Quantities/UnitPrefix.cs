using Galaxon.Core.Exceptions;

namespace Galaxon.Quantities;

public class UnitPrefix
{
    /// <summary>
    /// String of valid currency prefixes.
    /// </summary>
    public const string CurrencyPrefixes = "kKMBT";

    /// <summary>
    /// Metric prefixes - private backing field.
    /// </summary>
    private static List<UnitPrefix>? _metric;

    /// <summary>
    /// Binary prefixes - private backing field.
    /// </summary>
    private static List<UnitPrefix>? _binary;

    /// <summary>
    /// Currency prefixes - private backing field.
    /// </summary>
    private static List<UnitPrefix>? _currency;

    /// <summary>
    /// Private backing field for all prefixes.
    /// </summary>
    private static List<UnitPrefix>? _all;

    public UnitPrefix(string symbol, double multiplier)
    {
        Symbol = symbol;
        Multiplier = multiplier;
    }

    public string Symbol { get; }

    public double Multiplier { get; }

    /// <summary>
    /// Metric prefixes.
    /// <see href="https://en.wikipedia.org/wiki/Metric_prefix" />
    /// </summary>
    public static List<UnitPrefix> Metric =>
        _metric ??= new List<UnitPrefix>
        {
            new ("Q", 1e30),
            new ("R", 1e27),
            new ("Y", 1e24),
            new ("Z", 1e21),
            new ("E", 1e18),
            new ("P", 1e15),
            new ("T", 1e12),
            new ("G", 1e9),
            new ("M", 1e6),
            new ("k", 1e3),
            new ("h", 100),
            new ("da", 10),
            new ("d", 0.1),
            new ("c", 0.01),
            new ("m", 1e-3),
            new ("µ", 1e-6),
            new ("n", 1e-9),
            new ("p", 1e-12),
            new ("f", 1e-15),
            new ("a", 1e-18),
            new ("z", 1e-21),
            new ("y", 1e-24),
            new ("r", 1e-27),
            new ("q", 1e-30)
        };

    /// <summary>
    /// Subset of metric prefixes from (kilo) k upwards.
    /// Used by units such as tonne (t) and byte (B).
    /// </summary>
    public static List<UnitPrefix> LargeMetric => Metric.Where(p => p.Multiplier >= 1000).ToList();

    /// <summary>
    /// Subset of metric prefixes from milli (m) downwards.
    /// Used by units such as second (s).
    /// </summary>
    public static IEnumerable<UnitPrefix> SmallMetric => Metric.Where(p => p.Multiplier <= 0.001);

    /// <summary>
    /// Binary prefixes.
    /// <see href="https://en.wikipedia.org/wiki/Binary_prefix" />
    /// </summary>
    public static List<UnitPrefix> Binary =>
        _binary ??= new List<UnitPrefix>
        {
            new ("Ki", Pow(2, 10)),
            new ("Mi", Pow(2, 20)),
            new ("Gi", Pow(2, 30)),
            new ("Ti", Pow(2, 40)),
            new ("Pi", Pow(2, 50)),
            new ("Ei", Pow(2, 60)),
            new ("Zi", Pow(2, 70)),
            new ("Yi", Pow(2, 80))
        };

    /// <summary>
    /// Additional prefixes used by currencies.
    /// Both lower- and upper-case 'k' are in common usage for 1000, and B is used instead of G for
    /// billion. M (mega) works for million and T (tera) works for trillion.
    /// </summary>
    public static List<UnitPrefix> Currency =>
        _currency ??= new List<UnitPrefix>
        {
            new ("K", 1e3),
            new ("B", 1e9)
        };

    /// <summary>
    /// Get all the valid prefixes.
    /// </summary>
    public static List<UnitPrefix> All
    {
        get
        {
            if (_all != null)
            {
                return _all;
            }
            List<UnitPrefix> prefixSymbols = new ();
            prefixSymbols.AddRange(Metric);
            prefixSymbols.AddRange(Binary);
            prefixSymbols.AddRange(Currency);
            _all = prefixSymbols;
            return _all;
        }
    }

    public static UnitPrefix? Get(string symbol)
    {
        return All.FirstOrDefault(up => up.Symbol == symbol);
    }

    /// <summary>
    /// Given the symbol, get the multiplier value for a given prefix.
    /// Throws exception for an invalid prefix symbol.
    /// Non-nullable result can be used in expressions.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentInvalidException"></exception>
    public static double GetMultiplier(string symbol)
    {
        var prefix = Get(symbol);
        if (prefix == null)
        {
            throw new ArgumentInvalidException(nameof(symbol),
                $"Invalid prefix symbol '{symbol}'.");
        }
        return prefix.Multiplier;
    }

    public static List<UnitPrefix> GetMultiple(string csl)
    {
        var prefixes = csl.Split(',');
        List<UnitPrefix> result = new ();
        foreach (var prefix in prefixes)
        {
            var match = Get(prefix);
            if (match is null)
            {
                throw new ArgumentFormatException(nameof(csl),
                    $"Invalid format '{csl}'. It should be a comma-separated string of valid prefixes, without spaces, e.g. \"k,M,G\"");
            }
            result.Add(match);
        }
        return result;
    }

    public override string ToString()
    {
        return Symbol;
    }

    /// <summary>
    /// Combine 2 or more prefix groups into one.
    /// </summary>
    /// <param name="groups"></param>
    /// <returns></returns>
    public static List<UnitPrefix> Combine(params List<UnitPrefix>[] groups)
    {
        List<UnitPrefix> result = new ();
        foreach (var group in groups)
        {
            result.AddRange(group);
        }
        return result;
    }
}
