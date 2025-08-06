namespace WPF_Wave.Helpers;

public static class AdaptHelper
{
    /// <summary>
    /// 指定された値を、制限された範囲内に適応させるメソッド
    /// </summary>
    /// <param name="x">制限したい値</param>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値</param>
    /// <returns>minがmaxよりも大きい場合、不定値。xがminよりも小さい場合min。xがmaxよりも大きい場合max。</returns>
    public static double AdaptRestrictions(this double x, double min, double max)
    {
        return Math.Max(min, Math.Min(max, x));
    }

    /// <summary>
    /// 指定された値を、制限された範囲内に適応させるメソッド
    /// </summary>
    /// <param name="x">制限したい値</param>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値</param>
    /// <returns>minがmaxよりも大きい場合、不定値。xがminよりも小さい場合min。xがmaxよりも大きい場合max。</returns>
    public static TimeSpan AdaptRestrictions(this TimeSpan x, TimeSpan min, TimeSpan max)
    {
        if (min > max)
        {
            return TimeSpan.MaxValue; // 不定値
        }
        return TimeSpan.FromTicks(Math.Max(min.Ticks, Math.Min(max.Ticks, x.Ticks)));
    }
}

