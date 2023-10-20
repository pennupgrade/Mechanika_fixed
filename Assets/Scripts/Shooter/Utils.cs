using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

// Some overall helper methods we can add as we go
public static class Utils
{
    public static void ForeachIndex<T>(this List<T> l, Action<T, int> action)
    {
        for(int i=0; i<l.Count; i++)
            action(l[i], i);
    }

    /// <summary>
    /// Interpolate between two strings assuming s1 is a version of s2 with some RHS characters cutoff.
    /// </summary>
    /// <param name="s1"></param>
    /// <param name="s2"></param>
    /// <param name="charCount"></param>
    /// <returns></returns>
    public static string Stringlerp(string s1, string s2, int charCount)
        => s1.Length + charCount >= s2.Length ? s2 : s1 + s2.Substring(s1.Length, charCount);

    /// <summary>
    /// Cuts the string into size charCount where spaces don't contribute to size.
    /// </summary>
    /// <param name="s1"></param>
    /// <param name="s2"></param>
    /// <param name="charCount"></param>
    /// <returns></returns>
    public static string PartialStringSpaceTrim(string si, int charCount)
    {
        string so = ""; int charsLeft = charCount;
        for(int i=0; i<si.Length; i++)
        {
            so += si[i]; if (si[i] != ' ') charsLeft--;
            if (charsLeft <= 0) break;
        }

        return so;
    }

    /// <summary>
    /// Cut a string, ending at the RHS.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="amount"></param> [0, 1] 
    /// <returns></returns>
    public static string StringPercent(string s, float amount)
        => Stringlerp("", s, (int)(s.Length * amount));
}