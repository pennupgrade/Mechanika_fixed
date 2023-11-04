Shader "Unlit/Character"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _Aspect ("Aspect Ratio", Float) = 1.
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Common.hlsl"

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vOut
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            vOut vert (vIn v)
            {
                vOut o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            float _Aspect;

            fixed4 frag (vOut i) : SV_Target
            {
                float4 sample = tex2D(_MainTex, i.uv);

                // Dither
                float2 p = i.uv*2.-1.; p *= _Aspect;

                float repSize = .01;
                float2 sizeRange = float2(-.01, repSize*2.);
                float2 distRange = float2(-.3, -.8-.2);

                float2 lp = p;
                lp.y = amod(lp.y, repSize) - repSize*.5;
                float2 id = float2(0., p.y - lp.y);
                lp.x = amod(lp.x + id.y*2.34, repSize) - repSize*.5; id.x = p.x - lp.x;
                float size = sizeRange.x + (sizeRange.y - sizeRange.x) * pow(smoothstep(distRange.x, distRange.y, id.y),2.);
                float exists = 1.-sb(length(lp) - size);
                
                return float4(sample.rgb, min(sample.a, exists));
            }
            ENDCG
        }
    }
}
