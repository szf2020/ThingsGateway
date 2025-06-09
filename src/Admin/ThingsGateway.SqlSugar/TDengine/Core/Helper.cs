namespace SqlSugar.TDengineAdo;

internal static class Helper
{
    public static long DateTimeToLong19(DateTime dateTime)
    {
        DateTimeOffset dateTimeOffset = dateTime.Kind != DateTimeKind.Utc ? new DateTimeOffset(dateTime.ToUniversalTime()) : new DateTimeOffset(dateTime, TimeSpan.Zero);
        return dateTimeOffset.ToUnixTimeSeconds() * 1000000000L + dateTimeOffset.Ticks % 10000000L * 100L;
    }

    public static DateTime Long16ToDateTime(long timestampInMicroseconds)
    {
        long seconds = timestampInMicroseconds / 1000000L;
        long num = timestampInMicroseconds % 1000000L;
        return DateTimeOffset.FromUnixTimeSeconds(seconds).AddTicks(num * 10L).LocalDateTime;
    }

    public static DateTime Long19ToDateTime(long timestampInNanoseconds)
    {
        long seconds = timestampInNanoseconds / 1000000000L;
        long num = timestampInNanoseconds % 1000000000L;
        return DateTimeOffset.FromUnixTimeSeconds(seconds).AddTicks(num / 100L).LocalDateTime;
    }

    public static long DateTimeToLong16(DateTime dateTime)
    {
        DateTimeOffset dateTimeOffset = dateTime.Kind != DateTimeKind.Utc ? new DateTimeOffset(dateTime.ToUniversalTime()) : new DateTimeOffset(dateTime, TimeSpan.Zero);
        return dateTimeOffset.ToUnixTimeSeconds() * 1000000L + dateTimeOffset.Ticks % 10000000L / 10L;
    }

    public static long ToUnixTimestamp(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc ? new DateTimeOffset(dateTime).ToUnixTimeMilliseconds() : new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }
}
