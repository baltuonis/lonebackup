using System;
using System.Globalization;

namespace LoneBackup.App.Utils;

public static class TimeUtils
{
    public static string GetCurrentTimeUtcIso()
    {
        return DateTime.UtcNow.ToString("s", DateTimeFormatInfo.InvariantInfo);
    }
}