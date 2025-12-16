using System;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Helpers;

public static class AasPropertyExtensions
{
    public static string? GetText(this IProperty? prop)
    {
        if (prop == null) return null;
        return AasValueUnwrap.UnwrapToString(prop.Value);
    }

    public static string? GetText(this Property<string>? prop)
    {
        if (prop == null) return null;
        return AasValueUnwrap.UnwrapToString(prop.Value);
    }

    public static void SetText(this Property<string>? prop, string? value)
    {
        if (prop == null) return;
        prop.Value = new PropertyValue<string>(value ?? string.Empty);
    }

    public static int GetIntValue(this Property<int>? prop, int fallback = 0)
    {
        if (prop == null) return fallback;
        var val = AasValueUnwrap.UnwrapToInt(prop.Value);
        return val ?? fallback;
    }

    public static void SetIntValue(this Property<int>? prop, int value)
    {
        if (prop == null) return;
        prop.Value = new PropertyValue<int>(value);
    }

    public static void SetDoubleValue(this Property<double>? prop, double value)
    {
        if (prop == null) return;
        prop.Value = new PropertyValue<double>(value);
    }

    public static void SetBoolValue(this Property<bool>? prop, bool value)
    {
        if (prop == null) return;
        prop.Value = new PropertyValue<bool>(value);
    }

    public static bool IsNullOrWhiteSpace(this Property<string>? prop)
    {
        return string.IsNullOrWhiteSpace(prop.GetText());
    }

    public static double GetDoubleValue(this Property<double>? prop, double fallback = 0d)
    {
        if (prop == null) return fallback;
        var val = AasValueUnwrap.UnwrapToDouble(prop.Value);
        return val ?? fallback;
    }
}
