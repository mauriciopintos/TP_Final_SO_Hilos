using System;

public static class ControlTiempo
{
    // Timestamp mm:ss.fff
    public static string MarcaTiempo()
    {
        return DateTime.Now.ToString("mm':'ss'.'fff");
    }
}