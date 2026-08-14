using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum LightColorChannel
{
    None = 0,
    Red = 1,
    Green = 2,
    Blue = 4,
    Yellow = Red | Green,
    Magenta = Red | Blue,
    Cyan = Green | Blue,
    White = Red | Green | Blue
}

public static class LightColorHelper
{
    public static LightColorChannel Merge(LightColorChannel a, LightColorChannel b)
    {
        return a | b;
    }

    public static LightColorChannel Merge(IEnumerable<LightRayData> rays)
    {
        LightColorChannel merged = LightColorChannel.None;

        foreach (LightRayData ray in rays)
        {
            if (ray == null) continue;
            merged |= ray.lightColor;
        }

        return merged;
    }

    public static List<LightColorChannel> SplitBaseColors(LightColorChannel color)
    {
        List<LightColorChannel> result = new List<LightColorChannel>();

        if ((color & LightColorChannel.Red) != 0)
        {
            result.Add(LightColorChannel.Red);
        }

        if ((color & LightColorChannel.Green) != 0)
        {
            result.Add(LightColorChannel.Green);
        }

        if ((color & LightColorChannel.Blue) != 0)
        {
            result.Add(LightColorChannel.Blue);
        }

        if (result.Count == 0)
        {
            result.Add(LightColorChannel.None);
        }

        return result;
    }

    public static bool Matches(LightColorChannel incoming, LightColorChannel required, bool requireExact)
    {
        if (required == LightColorChannel.None)
        {
            return true;
        }

        return requireExact
            ? incoming == required
            : (incoming & required) == required;
    }

    public static Color ToUnityColor(LightColorChannel color)
    {
        switch (color)
        {
            case LightColorChannel.Red:
                return new Color(1f, 0.1f, 0.08f, 1f);
            case LightColorChannel.Green:
                return new Color(0.1f, 1f, 0.2f, 1f);
            case LightColorChannel.Blue:
                return new Color(0.1f, 0.35f, 1f, 1f);
            case LightColorChannel.Yellow:
                return new Color(1f, 0.92f, 0.1f, 1f);
            case LightColorChannel.Magenta:
                return new Color(1f, 0.12f, 0.9f, 1f);
            case LightColorChannel.Cyan:
                return new Color(0.1f, 0.95f, 1f, 1f);
            case LightColorChannel.White:
                return Color.white;
            default:
                return new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
}
