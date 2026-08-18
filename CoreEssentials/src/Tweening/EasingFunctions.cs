using System;

namespace CoreEssentials.Tweening;

/// <summary>
/// Collection of standard easing functions for use with tweens.
/// Each function takes a normalized time value (0 to 1) and returns an eased value (0 to 1).
/// </summary>
public static class EasingFunctions
{
    #region Linear

    /// <summary>No easing — constant speed.</summary>
    public static Func<float, float> Linear => t => t;

    #endregion

    #region In-Easing Functions

    /// <summary>Accelerates from rest (t²).</summary>
    public static Func<float, float> InQuad => t => t * t;

    /// <summary>Accelerates from rest (t³).</summary>
    public static Func<float, float> InCubic => t => t * t * t;

    /// <summary>Accelerates from rest (t⁴).</summary>
    public static Func<float, float> InQuart => t => t * t * t * t;

    /// <summary>Accelerates from rest (t⁵).</summary>
    public static Func<float, float> InQuint => t => t * t * t * t * t;

    /// <summary>Accelerates using sine wave.</summary>
    public static Func<float, float> InSine => t => (float)(1.0 - Math.Cos((t * Math.PI) / 2.0));

    /// <summary>Accelerates using exponential curve.</summary>
    public static Func<float, float> InExpo => t => t == 0 ? 0 : (float)Math.Pow(2.0, 10.0 * (t - 1.0));

    /// <summary>Accelerates using circular arc.</summary>
    public static Func<float, float> InCirc => t => (float)(1.0 - Math.Sqrt(1.0 - t * t));

    /// <summary>Accelerates with elastic overshoot.</summary>
    public static Func<float, float> InElastic => ElasticIn;

    /// <summary>Accelerates with back overshoot.</summary>
    public static Func<float, float> InBack => t =>
    {
        const float s = 1.70158f;
        return t * t * ((s + 1.0f) * t - s);
    };

    /// <summary>Accelerates with bounce effect.</summary>
    public static Func<float, float> InBounce => t => 1.0f - OutBounce(1.0f - t);

    #endregion

    #region Out-Easing Functions

    /// <summary>Decelerates to rest (t²).</summary>
    public static Func<float, float> OutQuad => t => (float)(t * (2.0 - t));

    /// <summary>Decelerates to rest (t³).</summary>
    public static Func<float, float> OutCubic => t => (float)((--t) * t * t + 1.0);

    /// <summary>Decelerates to rest (t⁴).</summary>
    public static Func<float, float> OutQuart => t => (float)(1.0 - (--t) * t * t * t);

    /// <summary>Decelerates to rest (t⁵).</summary>
    public static Func<float, float> OutQuint => t => (float)(1.0 + (--t) * t * t * t * t);

    /// <summary>Decelerates using sine wave.</summary>
    public static Func<float, float> OutSine => t => (float)Math.Sin((t * Math.PI) / 2.0);

    /// <summary>Decelerates using exponential curve.</summary>
    public static Func<float, float> OutExpo => t => t == 1 ? 1 : (float)(1.0 - Math.Pow(2.0, -10.0 * t));

    /// <summary>Decelerates using circular arc.</summary>
    public static Func<float, float> OutCirc => t => (float)Math.Sqrt(1.0 - (t - 1.0) * (t - 1.0));

    /// <summary>Decelerates with elastic overshoot.</summary>
    public static Func<float, float> OutElastic => ElasticOut;

    /// <summary>Decelerates with back overshoot.</summary>
    public static Func<float, float> OutBack => t =>
    {
        const float s = 1.70158f;
        return (--t) * t * ((s + 1.0f) * t + s) + 1.0f;
    };

    /// <summary>Decelerates with bounce effect.</summary>
    public static Func<float, float> OutBounce => t => BounceOut(t);

    #endregion

    #region In-Out-Easing Functions

    /// <summary>Accelerates then decelerates (t²).</summary>
    public static Func<float, float> InOutQuad => t =>
    {
        if (t < 0.5f)
            return 2f * t * t;
        return -1f + (4f - 2f * t) * t;
    };

    /// <summary>Accelerates then decelerates (t³).</summary>
    public static Func<float, float> InOutCubic => t =>
    {
        if (t < 0.5f)
            return 4f * t * t * t;
        return (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
    };

    /// <summary>Accelerates then decelerates (t⁴).</summary>
    public static Func<float, float> InOutQuart => t =>
    {
        if (t < 0.5f)
            return 8f * t * t * t * t;
        return -1f / 2f * ((t = t * 2f - 3f) * t * t * t - 2f);
    };

    /// <summary>Accelerates then decelerates (t⁵).</summary>
    public static Func<float, float> InOutQuint => t =>
    {
        if (t < 0.5f)
            return 16f * t * t * t * t * t;
        return 1f / 2f * ((t = t * 2f - 2f) * t * t * t * t + 2f);
    };

    /// <summary>Accelerates then decelerates using sine wave.</summary>
    public static Func<float, float> InOutSine => t => (float)(-0.5f * (Math.Cos(Math.PI * t) - 1.0));

    /// <summary>Accelerates then decelerates using exponential curve.</summary>
    public static Func<float, float> InOutExpo => t => { if (t == 0) return 0; if (t == 1) return 1; float h = t * 2.0f; return h < 1.0f ? 0.5f * (float)Math.Pow(2.0, 10.0 * (h - 1.0)) : 0.5f * (float)(-Math.Pow(2.0, -10.0 * (h - 1.0)) + 2.0); };

    /// <summary>Accelerates then decelerates using circular arc.</summary>
    public static Func<float, float> InOutCirc => t => { float h = t * 2.0f; return h <= 1.0f ? -0.5f * (float)(Math.Sqrt(1.0 - h * h) - 1.0) : 0.5f * (float)(Math.Sqrt(1.0 - (h -= 2.0f) * h) + 1.0); };

    /// <summary>Accelerates then decelerates with elastic overshoot.</summary>
    public static Func<float, float> InOutElastic => t => { if (t == 0) return 0; if (t == 1) return 1; float h = t * 2.0f; if (h < 1.0f) return 0.5f * ElasticIn(h); return 0.5f * ElasticOut(h - 1.0f) + 0.5f; };

    /// <summary>Accelerates then decelerates with back overshoot.</summary>
    public static Func<float, float> InOutBack => t =>
    {
        const float s = 1.70158f;
        float h = t * 2.0f;
        if (h <= 1.0f)
            return 0.5f * (h * h * ((s + 1.0f) * h - s));
        h -= 2.0f;
        return 0.5f * (h * h * ((s + 1.0f) * h + s) + 2.0f);
    };

    /// <summary>Accelerates then decelerates with bounce effect.</summary>
    public static Func<float, float> InOutBounce => t => { float h = t * 2.0f; return h <= 1.0f ? 0.5f * (1.0f - BounceOut(1.0f - h)) : 0.5f * BounceOut(h - 1.0f) + 0.5f; };

    #endregion

    #region Private Helpers

    private static float ElasticIn(float t)
    {
        if (t == 0 || t == 1) return t;
        return (float)(-Math.Pow(2.0, 10.0 * (t - 1.0)) * Math.Sin((t - 1.1) * 5.0 * Math.PI));
    }

    private static float ElasticOut(float t)
    {
        if (t == 0 || t == 1) return t;
        return (float)(Math.Pow(2.0, -10.0 * t) * Math.Sin((t - 0.1) * 5.0 * Math.PI) + 1.0f);
    }

    private static float BounceOut(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1.0f / d1)
            return n1 * t * t;
        else if (t < 2.0f / d1)
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        else if (t < 2.5f / d1)
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        else
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }

    #endregion
}
