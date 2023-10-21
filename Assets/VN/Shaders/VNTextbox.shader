Shader "Unlit/VN/OutputTextbox"
{
    Properties
    {
        [HideInInspector] _Dimensions ("Dimensions", Vector) = (1.,1.,0.,0.)
        
        _HighlightInset ("Highlight Inset", Float) = 0.1
        _HighlightThickness ("Highlight Thickness", Float) = 0.1

        _InnerHighlightInset ("Inner Highlight Inset", Float) = 0.3
        _InnerHighlightThickness ("Inner Highlight Thickness", Float) = 0.08

        _OuterHighlightColor ("Outer Highlight Color", Color) = (1., 1., 1., 1.)
        _InnerHighlightColor ("Inner Highlight Color", Color) = (0.8, 0.8, 0.8, 1.)
        _NonHighlightColor1 ("Non-Highlight Color 1", Color) = (0., 0., 0., 0.7)
        _NonHighlightColor2 ("Non-Highlight Color 2", Color) = (0.15, 0.15, 0.15, 0.7)

        _HighlightDimensions ("Highlight Dimensions", Vector) = (4., 2., 8., 4.)
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

            float _InnerHighlightInset;
            float _InnerHighlightThickness;

            float4 _OuterHighlightColor;
            float4 _InnerHighlightColor;
            float4 _NonHighlightColor1;
            float4 _NonHighlightColor2;

            float4 _HighlightDimensions;

            float cover;

            float sbHighlight(float2 p, float2 ps, float2 pe, float2 domainDim, float2 highDim, float t)
            {

                //
                p -= ps;
                float2 fo = normalize(pe - ps);
                float2 up = perp(fo);
                p = float2(dot(p, fo), dot(p, up));

                p.x = amod(p.x - t, domainDim.x) - domainDim.x*.5;
                p.y = abs(p.y);
                float newCover = max(cover, step(p.y, (domainDim+highDim)*.5));

                p = p - float2(0., domainDim.y*.5);
                p = abs(p);

                float b = step(p.x, highDim.x*.5) * step(p.y, highDim.y*.5) * (1.-cover);
                cover = newCover; return b;
            }

            float sbLine(float2 p, float2 ps, float2 pe, float thickness)
            {
                //
                p -= ps;
                float2 fo = normalize(pe - ps);
                float2 up = perp(fo);
                p = float2(dot(p, fo), dot(p, up));

                //
                return step(abs(p.y - thickness*.5), thickness*.5);
            }

            float sdHighlightbox(float2 p, float )

            fixed4 frag(vOut i) : SV_Target
            {
                float2 p = (i.uv - .5) * _Dimensions;

                float isHighlight = 0.;

                // Simple Highlight
                float bOuter = sb(sdBox(p, _Dimensions - _HighlightInset*2.));
                float bInner = sb(sdBox(p, _Dimensions - (_HighlightInset+_HighlightThickness)*2.));
                float bSimpleHighlight = bOuter * (1.-bInner);
                isHighlight += bSimpleHighlight;

                // Inner Highlight
                float bOuterBox = sb(sdBox(p, _Dimensions - _InnerHighlightInset*2.));
                float bInnerBox = sb(sdBox(p, _Dimensions - (_InnerHighlightInset + _InnerHighlightThickness)*2.));
                float bInnerHighlight = bOuterBox * (1.-bInnerBox);
                isHighlight += bInnerHighlight;

                // Dotted Highlights - Only compatible with one Dimensions.. restrict in C# at least.. assume for dimension you already have for nowhn
                // (980, 197), (490, 98.5)
                float t = _Time.y*10.;
                cover = 1.-bInnerBox;

                float2 hDim = _HighlightDimensions.xy;
                float2 dDim = _HighlightDimensions.zw;

                isHighlight += sbHighlight(p, float2(-490, 35.), float2(-390.-10., 115.), dDim, hDim, t);
                isHighlight += sbHighlight(p, float2(-500., -25.), float2(-420, -105.), dDim, hDim, -t);
                //isHighlight += sbHighlight(p, float2(415., 115.), float2(475., 40.), dDim, hDim, t);
                //isHighlight += sbHighlight(p, float2(490., -35.), float2(415., -95.), dDim, hDim, t);
                isHighlight += sbHighlight(p, float2(490, 35.), float2(400., 115.), dDim, hDim, -t);
                isHighlight += sbHighlight(p, float2(500., -25.), float2(420., -105.), dDim, hDim, t);

                isHighlight = saturate(isHighlight);

                // Final Color
                float4 finalCol = lerp(_NonHighlightColor1, _InnerHighlightColor, isHighlight);

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