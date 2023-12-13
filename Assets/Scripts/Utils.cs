using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using Utilities;
using static Utilities.Utils;

using static Utilities.MathUtils;
using System.Collections;

namespace Utilities
{
    public static partial class Utils
    {

        public static T GetValue<K, T>(this Dictionary<K, T> dict, K key, T failVal)
            => dict.TryGetValue(key, out T val) ? val : failVal;

        public static void ForLength(int length, Action<int> action)
        { for(int i=0; i<length; i++) { action(i); } }
        public static void ForLength(int length, Action action)
            => ForLength(length, i => action());

        public static List<T> Copy<T>(this List<T> L)
        {
            List<T> list = new();
            L.ForEach(e => list.Add(e));
            return list;
        }

        public static void Print<T>(this IEnumerable<T> E)
        {
            foreach (T e in E)
                Debug.Log(e);
        }

        public static void ForEach<K, V>(this Dictionary<K, List<V>> dict, Action<V> action)
        {
            foreach (KeyValuePair<K, List<V>> kvp in dict)
                kvp.Value.ForEach(action);
        }

        public static void ForEachIndex<K, V>(this Dictionary<K, List<V>> dict, Action<V, int> action)
        {
            int curr = 0;
            foreach (KeyValuePair<K, List<V>> kvp in dict)
            {
                for (int i = curr; i < curr + kvp.Value.Count; i++)
                    action.Invoke(kvp.Value[i - curr], i);
                curr += kvp.Value.Count;
            }
        }

        public static void RemoveAt<K, V>(this Dictionary<K, List<V>> dict, int i)
        {
            foreach(KeyValuePair<K, List<V>> kvp in dict)
            {
                i -= kvp.Value.Count;
                if (i < 0) kvp.Value.RemoveAt(i+kvp.Value.Count);
            }
        }

        public static void ForEachIndex<T>(this IList<T> list, Action<T, int> action)
        {
            for (int i = 0; i < list.Count; i++)
                action.Invoke(list[i], i);
        }

        public static void ForEachParallel<T>(this IEnumerable<T> E, Action<T> action)
        {
            throw new NotImplementedException();
        }

        public static void ForEach<K, V>(this IEnumerable<KeyValuePair<K, V>> E, Action<V> action)
        { foreach (KeyValuePair<K, V> kvp in E) action(kvp.Value); }

        public static void ForEach<K, V>(this IEnumerable<KeyValuePair<K, V>> E, Action<K, V> action)
        { foreach (KeyValuePair<K, V> kvp in E) action(kvp.Key, kvp.Value); }

    }

    public static partial class Utils
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

        public static readonly Mesh Quad = new()
        {
            vertices = new Vector3[]
                { new(0f, -3f), new(0f, 3f), new(1f, -3f), new(1f, 3f) },
            uv = new Vector2[]
                { new(0f, -3f), new(0f, 3f), new(1f, -3f), new(1f, 3f) },
            triangles = new int[]
                { 0, 1, 2, 1, 2, 3 }
        };

        public static readonly Mesh ExtendedQuad = new()
        {
            vertices = new Vector3[]
                { new(-4f, -4f), new(-4f, 4f), new(4f, -4f), new(4f, 4f) },
            uv = new Vector2[]
                { new(-4f, -4f), new(-4f, 4f), new(4f, -4f), new(4f, 4f) },
            triangles = new int[]
                { 0, 1, 2, 1, 2, 3 }
        };

        public static float amod(float f, float m)
        {
            float s = fmod(abs(f), m);
            return s + step(f, 0f) * (m - 2f*s);
        }

        public static float2 toPolar(float2 cart)
            => float2(length(cart), amod(atan2(cart.y, cart.x), TAU));
        public static float2 toCartesian(float2 polar)
            => polar.x * float2(cos(polar.y), sin(polar.y));
    }

    public static partial class Utils
    {

        public static IEnumerator WaitThenAction(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }

    }

    public static class EnemyUtils
    {
        public static int2 AngleDegreesToFourOrientation(float a)
        {
            float repLen = math.PI * .5f;
            float2 orientation = toCartesian(new float2(1f, a*D2R - (amod(a*D2R + repLen*.5f, repLen) - repLen*1.5f)));
            
            return new int2
            (
                Mathf.RoundToInt(orientation.x),
                Mathf.RoundToInt(orientation.y)
            );
        }
    }

    public static class Collision
    {
        public static bool CircleCircle(float2 c1, float r1, float2 c2, float r2)
            => length(c2 - c1) - r1 - r2 <= 0f;

        public static float2? PointCollideArena(float2 boundSize, float2 p, float r)
        {
            float2? wallNormal = null;

            float2 lp = p;
            lp = abs(lp);
            lp = boundSize*.5f - lp;

            float minDist = min(lp.x, lp.y) - r;

            if(minDist <= 0)
            {
                bool isXWall = lp.x < lp.y;

                wallNormal =  isXWall ? float2(-1f, 0f) : float2(0f, -1f);
                wallNormal *= isXWall ? step(0f, p.x)*2f-1f : step(0f, p.y)*2f-1f;
            }

            return wallNormal;
        }
        public static float2? PointCollideArena(BulletEngine engine, float2 p, float r) 
            => PointCollideArena(engine.BoundSize, p, r);
    }

    public static class MathUtils
    {

        public const float D2R = .0174533f;
        public const float R2D = 57.29578f;

        public const float TAU = 6.2831f;

        public static float2 PolarToCartesian(float o)
            => float2(cos(o), sin(o));

        public static float2 PolarToCartesian(this float2 polar)
            => polar.y * float2(cos(polar.x), sin(polar.x));

        public static float2 Rotate(float o, float2 v)
        {
            float r = length(v);
            return cos(o) * v + sin(o) * sin(o) * cross(v.xyz(), float3(0f, 0f, -1f)).xy();
        }

    }

    public static class Swizzle
    {
        public static float2 xy(this float3 v)
            => new float2(v.x, v.y);

        public static float2 xy(this Vector3 v)
            => new float2(v.x, v.y);

        public static float3 xyz(this float2 v)
            => new float3(v.x, v.y, 0f);

        public static float3 xyz(this float2 v, float z)
            => new float3(v.x, v.y, z);

        public static float4 xyzw(this float3 v)
            => new float4(v.x, v.y, v.z, 0f);

        public static float3 xyz(this Vector2 v, float z = 0f) 
        => new float3(v.x, v.y, z);

        public static Vector4 xyzw(this Vector3 v, float w = 0f) 
        => new(v.x, v.y, v.z, w);

        public static float4 xyzw(this float3 v, float w = 0f) 
        => new(v.x, v.y, v.z, w);

    }

    public static class MeshUtils
    {
        public static readonly Mesh QuadMesh = new Mesh()
        {
            vertices = new Vector3[]
        {
            new(-8, -8, 0),
            new(-8, 8, 0),
            new(8, -8, 0),
            new(8, 8, 0)
        },
            triangles = new int[]
        {
            0, 1, 3,
            0, 2, 3
        },
            uv = new Vector2[]
        {
            new(-8, -8),
            new(-8, 8),
            new(8, -8),
            new(8, 8)
        }
        };
    }

    public class InfiniteMaterialBlock
    {

        const int MAXBLOCKSIZE = 1023;
        readonly List<MaterialPropertyBlock> blocks = new();
        int instanceCount; //bit jank how we're doing this
        readonly Dictionary<string, int> Properties = new();

        void EnsureEnoughBlocks(int blocksNeeded) { for (int i = blocks.Count; i < blocksNeeded; i++) blocks.Add(new()); }
        int BlocksNeeded(int arrLen) => Mathf.CeilToInt( ((float) arrLen) / MAXBLOCKSIZE);

        public void SetFloatArray(string name, float[] arr)
        {

            int maxIndex = Properties.GetValue(name, -1);

            int blocksNeeded = BlocksNeeded(arr.Length); EnsureEnoughBlocks(blocksNeeded); 
            if (maxIndex < blocksNeeded - 1) { blocks[blocksNeeded - 1].SetFloatArray(name, new float[MAXBLOCKSIZE]); Properties[name] = blocksNeeded - 1; }
            Utils.ForLength(blocksNeeded, i => blocks[i].SetFloatArray(name, arr[(i * MAXBLOCKSIZE)..min(arr.Length, (i+1) * MAXBLOCKSIZE)]));
            instanceCount = arr.Length;

        }
        public void SetVectorArray(string name, Vector4[] arr)
        {

            int maxIndex = Properties.GetValue(name, -1);

            int blocksNeeded = BlocksNeeded(arr.Length); EnsureEnoughBlocks(blocksNeeded);
            if (maxIndex < blocksNeeded - 1) { blocks[blocksNeeded - 1].SetVectorArray(name, new Vector4[MAXBLOCKSIZE]); Properties[name] = blocksNeeded - 1; }
            Utils.ForLength(blocksNeeded, i => blocks[i].SetVectorArray(name, arr[(i * MAXBLOCKSIZE)..min(arr.Length, (i+1) * MAXBLOCKSIZE)]));
            instanceCount = arr.Length;

        }

        /*public void Test()
        {
            MaterialPropertyBlock block = new();
            block.set
        }*/

        public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int layer, Camera camera)
            => blocks.ForEachIndex((b, i)
                => Graphics.DrawMeshInstanced(mesh, submeshIndex, material, matrices[(i * MAXBLOCKSIZE)..min((i + 1) * MAXBLOCKSIZE, instanceCount)], i < blocks.Count - 1 ? MAXBLOCKSIZE : instanceCount - MAXBLOCKSIZE * i, b, UnityEngine.Rendering.ShadowCastingMode.Off, false, layer, camera)
            );

    }
}