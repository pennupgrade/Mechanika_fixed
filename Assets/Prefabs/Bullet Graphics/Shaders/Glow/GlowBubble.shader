Shader "Unlit/Bullet/GlowBubble"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (1., 1., 1., 1.)
        _GlowAmplitude ("Glow Amplitude", Float) = 1.
        _GlowStart ("Glow Start", Float) = .2
        _GlowScale ("Glow Scale", Float) = .1

        _FinalGlowCut ("Final Glow Cut", Vector) = (0., .95, 0., 0.)
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

            float4 _GlowColor;
            float _GlowAmplitude;
            float _GlowStart;
            float _GlowScale;

            float2 _FinalGlowCut;

            vOut vert (vIn v)
            {
                vOut o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag (vOut i) : SV_Target
            {

                //
                float2 p = i.uv*2.-1.;
                float r = length(p);
                float emission = smoothstep(_FinalGlowCut.y, _FinalGlowCut.x, r)*getGlow(r, _GlowStart, _GlowScale)*_GlowAmplitude;

                return float4(emission*_GlowColor.rgb, pow(emission, 2.));
            }
            ENDCG
        }
    }
}
