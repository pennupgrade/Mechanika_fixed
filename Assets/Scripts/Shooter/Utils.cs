using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine;

public static class Utils
{
    public static void ForeachIndex<T>(this List<T> l, Action<T, int> action)
    {
        for (int i = 0; i < l.Count; i++)
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
        string endSave = ""; int endEnclose = -1;
        Stack<(string, int)> stack = new();
        for (int i = 0; i < si.Length; i++)
        {
            while (si[i] == '<')
            {
                int ii = si[i..].IndexOf('>') + i + 1;
                so += si[i..(ii)]; i = ii;

                endEnclose = si[i..].IndexOf('<') + i;
                stack.Append
                    ((
                        si[endEnclose..(si[i..].IndexOf('>') + i + 1)],
                        endEnclose
                    ));
            }

            
            so += si[i]; if (si[i] != ' ') charsLeft--;
            if (charsLeft <= 0) break;

            while (stack.TryPeek(out (string, int) item) && i == item.Item2 - 1)
            {
                i = si[i..].IndexOf('>') + i;
                so += item.Item1;

                stack.Pop();
            }
        }

        return so + endSave;
    }

    /// <summary>
    /// Cut a string, ending at the RHS.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="amount"></param> [0, 1] 
    /// <returns></returns>
    public static string StringPercent(string s, float amount)
        => Stringlerp("", s, (int)(s.Length * amount));

    public static float2 xy(this Vector3 v) => new (v.x, v.y);
    public static float2 xy(this float3 v) => new(v.x, v.y);
    public static float3 xyz(this Vector2 v, float z = 0f) => new float3(v.x, v.y, z);
    public static float3 xyz(this float2 v, float z = 0f) => new float3(v.x, v.y, z);
    public static Vector4 xyzw(this Vector3 v, float w = 0f) => new(v.x, v.y, v.z, w);
    public static float4 xyzw(this float3 v, float w = 0f) => new(v.x, v.y, v.z, w);
}