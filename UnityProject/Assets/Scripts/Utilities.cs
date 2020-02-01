using System;
using UnityEngine;

public class Utilities
{
    public static bool LoggingOn = false;

    public static void Log(string logstring)
    {
        if (LoggingOn) Debug.Log(logstring);
    }

    public static void LogError(string logstring)
    {
        if (LoggingOn) Debug.LogError(logstring);
    }

    public static void Assert(bool condition)
    {
        if (LoggingOn) Debug.Assert(condition);
    }
}

