Shader "Unlit/VN/OutputTextbox"
{
    Properties
    {
        [HideInInspector] _Dimensions ("Dimensions", Vector) = (1.,1.,0.,0.)
        
        _HighlightInset ("Highlight Inset", Float) = 0.1
        _HighlightThickness ("Highlight Thickness", Float) = 0.1

        _PHighlightInset ("Partial Highlight Inset", Float) = 0.3
        _PHighlightThickness ("Partial Highlight Thickness", Float) = 0.08
        _PHighlightVelocity ("Partial Highlight Velocity", Float) = 0.1
        _PHighlightSize ("Partial Highlight Size", Float) = 0.4 // Angle kinda jank cuz wide rectangle whatever for now

        _SimpleHighlightColor ("Highlight Color", Color) = (1., 1., 1., 1.)
        _PartialHighlightColor ("Partial Highlight Color", Color) = (0.8, 0.8, 0.8, 1.)
        _NonHighlightColor1 ("Non-Highlight Color 1", Color) = (0., 0., 0., 0.7)
        _NonHighlightColor2 ("Non-Highlight Color 2", Color) = (0.15, 0.15, 0.15, 0.7)
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

            vOut vert (vIn v)
            {
                vOut o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 _Dimensions;

            float _HighlightInset;
            float _HighlightThickness;

            float _PHighlightInset;
            float _PHighlightThickness;
            float _PHighlightVelocity;
            float _PHighlightSize;

            float4 _HighlightColor;
            float4 _NonHighlightColor;

            fixed4 frag(vOut i) : SV_Target
            {
                float2 p = (i.uv - .5) * _Dimensions;

                float isHighlight = 0.;

                // Simple Highlight
                float bOuter = sb(sdBox(p, _Dimensions - _HighlightInset*2.));
                float bInner = sb(sdBox(p, _Dimensions - (_HighlightInset+_HighlightThickness)*2.));
                float bSimpleHighlight = bOuter * (1.-bInner);
                isHighlight += bSimpleHighlight;

                // Partial Highlight
                float bAngle = sb(sdAngledLines(p, _Time.y*_PHighlightVelocity, _PHighlightSize));
                float bOuterBox = sb(sdBox(p, _Dimensions - _PHighlightInset*2.));
                float bInnerBox = sb(sdBox(p, _Dimensions - (_PHighlightInset + _PHighlightThickness)*2.));
                float bPartialHighlight = bAngle * bOuterBox * (1.-bInnerBox);
                isHighlight += bPartialHighlight;

                isHighlight = saturate(isHighlight);

                // Final Color
                float4 finalCol = lerp(_NonHighlightColor, _HighlightColor, isHighlight);

                return finalCol;

                //discard the idea of doing angled partial, change partial to inner, put highlight going through corners
                //place for portrait or name, ontop of everything and overrides everything, probably a equilateral rhombus with highlights
                //mabe can also use actual good partial, but also make it so it thickens the inner highlight in a linear to flat fashion so that inner highlight is consistent
                // and the line highlights will always be able to come out of the inner highlights

            }
            ENDCG
        }
    }
}