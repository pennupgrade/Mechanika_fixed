using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine;
using Unity.VisualScripting;

using static Unity.Mathematics.math;

using static Utils;

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
                        si[endEnclose..(si[(i+1)..].IndexOf('>') + (i+1) + 1)],
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

    public static readonly Mesh Quad = new()
    {
        vertices = new Vector3[]
            { new(0f, -3f), new(0f, 3f), new(1f, -3f), new(1f, 3f) },
        uv = new Vector2[]
            { new(0f, -3f), new(0f, 3f), new(1f, -3f), new(1f, 3f) },
        triangles = new int[]
            { 0, 1, 2, 1, 2, 3 }
    };

    public static float amod(float f, float m)
    {
        float s = fmod(abs(f), m);
        return s + step(f, 0f) * (m - 2f*s);
    }

    public const float D2R = 0.01745f;
    public const float R2D = 57.296f;

    public const float PI = 3.141592f;
    public const float TAU = 6.28318f;

    public static float2 toPolar(float2 cart)
        => float2(length(cart), amod(atan2(cart.y, cart.x), TAU));
    public static float2 toCartesian(float2 polar)
        => polar.x * float2(cos(polar.y), sin(polar.y));
}

public static class EnemyUtils
{
    public static int2 AngleDegreesToFourOrientation(float a)
    {
        float repLen = Utils.PI * .5f;
        float2 orientation = toCartesian(new float2(1f, a*D2R - (amod(a*D2R + repLen*.5f, repLen) - repLen*1.5f)));
        
        return new int2
        (
            Mathf.RoundToInt(orientation.x),
            Mathf.RoundToInt(orientation.y)
        );
    }
}