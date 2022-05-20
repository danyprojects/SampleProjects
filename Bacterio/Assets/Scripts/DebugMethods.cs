using System;
using System.Diagnostics;
using UnityEngine;

//Wrapper Debug
public static class WDebug
{
    [Conditional("DEBUG_BUILD")]
    public static void Log(string text)
    {
        UnityEngine.Debug.Log(Time.time + ": " + text);
    }

    [Conditional("DEBUG_BUILD")]
    public static void LogWarn(string text)
    {
        UnityEngine.Debug.LogWarning(Time.time + ": " + text);
    }

    [Conditional("DEBUG_BUILD"), Conditional("VERBOSE_LOG")]
    public static void LogVerb(string text)
    {
        UnityEngine.Debug.Log(Time.time + ": " + text);
    }

    [Conditional("DEBUG_BUILD")]
    public static void LogC(bool condition, string text)
    {
        if (condition)
            UnityEngine.Debug.LogWarning(Time.time + ": " + text);
    }

    [Conditional("DEBUG_BUILD")]
    public static void LogCWarn(bool condition, string text)
    {
        if(condition)
            UnityEngine.Debug.LogWarning(Time.time + ": " + text);
    }


    [Conditional("DEBUG_BUILD")]
    public static void Assert(bool condition, object message)
    {
        UnityEngine.Debug.Assert(condition, Time.time + ": " + message);
    }

    [Conditional("DEBUG_BUILD")]
    public static void Assert(Func<bool> conditionFunc, object message)
    {
        UnityEngine.Debug.Assert(conditionFunc(), Time.time + ": " + message);
    }

    public static void LogError(string text)
    {
        UnityEngine.Debug.LogError(Time.time + ": " + text);
    }
}

