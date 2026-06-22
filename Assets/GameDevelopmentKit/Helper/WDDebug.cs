using UnityEngine;

public class Logger
{
    private const string LOG_PREFIX = "<color=#12e4c9>WDG_SDK:</color>";

#if ENV_PROD
    [System.Diagnostics.Conditional("FALSE")]
#endif
    public static void Log(string msg)
    {
        Debug.Log(LOG_PREFIX + msg);
    }


#if ENV_PROD
    [System.Diagnostics.Conditional("FALSE")]
#endif
    public static void LogError(string msg)
    {
        Debug.LogError(LOG_PREFIX + msg);
    }

#if ENV_PROD
    [System.Diagnostics.Conditional("FALSE")]
#endif
    public static void LogWarning(string msg)
    {
        Debug.LogWarning(LOG_PREFIX + msg);
    }

#if ENV_PROD
    [System.Diagnostics.Conditional("FALSE")]
#endif
    public static void LogFormat(string msg, params object[] args)
    {
        Debug.LogFormat(LOG_PREFIX + msg, args);
    }
}