Shader "Unlit/Bullet/BulletTextureGlow"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}

        _Tint ("Tint", Color) = (1.,1.,1.,1.)

        _GlowColor ("Glow Color", Color) = (1., 1., 1., 1.)
        _GlowAmplitude ("Glow Amplitude", Float) = 1.
        _GlowStart ("Glow Start", Float) = .2
        _GlowScale ("Glow Scale", Float) = .1
        _GlowOverlap ("Glow Overlap", Float) = 1.

        _Scale ("Scale", Float) = 1.
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
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
            #include "../../../../VN/Shaders/Common.hlsl"

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

            float4 _Tint;

            float4 _GlowColor;
            float _GlowAmplitude;
            float _GlowStart;
            float _GlowScale;
            float _GlowOverlap;

            float _Scale;

            vOut vert (vIn v)
            {
                vOut o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (vOut i) : SV_Target
            {
                //
                float scale = _Scale;
                float2 texUV = (i.uv - .5)/scale + .5;
                float4 texCol = tex2D(_MainTex, texUV)*_Tint;
                texCol.rgb *= 1.-step(texCol.a, 0.);
                
                //Cutting ends
                float2 stuv = abs(texUV - .5);
                texCol *= step(stuv.x, 0.5) * step(stuv.y, 0.5);

                //
                float2 p = i.uv*2.-1.;
                float r = length(p);
                float emission = smoothstep(0.95, 0., r)*getGlow(r, _GlowStart, _GlowScale)*_GlowAmplitude;
                emission *= (step(texCol.a, 0.) - 1.)*(1.-_GlowOverlap) + 1.;

                return float4(texCol.rgb + emission*_GlowColor.rgb, max(texCol.a, emission));
            }
            ENDCG
        }
    }
}
