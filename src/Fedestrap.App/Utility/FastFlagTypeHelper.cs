using System;

namespace Fedestrap.Utility;

// Infers the value type a Roblox FastFlag expects from its name prefix, per
// Roblox's own naming convention (FFlag/FInt/FString/FLog/FDouble, each with
// a "DF" or "SF" variant). Used to give a soft heads up when a value clearly
// doesn't match, not to block anything: Roblox itself is the only real judge
// of whether a flag or value is valid.
public static class FastFlagTypeHelper
{
    public enum FlagKind
    {
        Unknown,
        Bool,
        Int,
        String,
        Double,
        Log
    }

    public static FlagKind InferKind(string name)
    {
        if (string.IsNullOrEmpty(name))
            return FlagKind.Unknown;
        if (StartsWithAny(name, "DFLog", "FLog"))
            return FlagKind.Log;
        if (StartsWithAny(name, "DFFlag", "SFFlag", "FFlag"))
            return FlagKind.Bool;
        if (StartsWithAny(name, "DFInt", "FInt"))
            return FlagKind.Int;
        if (StartsWithAny(name, "DFString", "FString"))
            return FlagKind.String;
        if (StartsWithAny(name, "DFDouble", "FDouble"))
            return FlagKind.Double;
        return FlagKind.Unknown;
    }

    private static bool StartsWithAny(string name, params string[] prefixes)
    {
        foreach (string prefix in prefixes)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    // Returns a short warning to show the user, or null when the value looks
    // fine (including when the flag's kind can't be determined at all).
    public static string? Validate(string name, string value)
    {
        FlagKind kind = InferKind(name);
        switch (kind)
        {
            case FlagKind.Bool:
                if (!string.Equals(value, "True", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(value, "False", StringComparison.OrdinalIgnoreCase))
                    return "'" + name + "' looks like a boolean flag (FFlag/DFFlag), but the value isn't True or False.";
                break;
            case FlagKind.Int:
                if (!long.TryParse(value, out _))
                    return "'" + name + "' looks like an integer flag (FInt/DFInt), but the value isn't a whole number.";
                break;
            case FlagKind.Double:
                if (!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                    return "'" + name + "' looks like a decimal flag (FDouble/DFDouble), but the value isn't a number.";
                break;
            case FlagKind.Log:
                if (!int.TryParse(value, out _))
                    return "'" + name + "' looks like a log level flag (FLog/DFLog), which is usually a whole number.";
                break;
        }
        return null;
    }
}
