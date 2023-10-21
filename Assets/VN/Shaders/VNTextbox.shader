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

            float hasBG;

            float sbPartition(float2 p, float2 ps, float2 pe)
            {
                return step(dot(p - ps, perp(-normalize(pe - ps))), 0.);
            }

            float2 sbLineDist(float2 p, float2 ps, float2 pe, float thickness)
            {
	            p -= ps;
	            float2 fo = normalize(pe - ps);
	            float2 up = perp(fo);
	            p = float2(dot(p, fo), dot(p, up));

	            float dist = length(pe-ps);
	            float distAlong = p.x;
	            float onSeg = step(distAlong, dist+thickness*.5) * step(0.-thickness*.5, distAlong);

	            return float2(step(abs(p.y), thickness*.5) * onSeg, p.x * onSeg - (1.-onSeg));
            }

            float sbPolygonHighlight(float2 p, float thickness, float offset, float size, float xc, float yc, int repCount, float repOffset, bool cutBG)
            {
               
                float lhasBG = 1.;

	            float2 data;
	            float cDist = -1.;

                float xcc = 2.*xc; //float xc = 4.7; float xcc = 11.2;
                float ycc = 2.*yc; //float yc = .9; float ycc = 1.8;
                float cs = .3; //cool size
                float sq = sqrt(2.);
	            
                float2 p1 = float2(-xc, -yc);
                float2 p2 = float2(xc-cs, -yc);
                float2 p3 = float2(xc, -yc+cs);
                float2 p4 = float2(xc, yc);
                float2 p5 = float2(-xc+cs, yc);
                float2 p6 = float2(-xc, yc-cs);
                
                float d1 = 0.;
                float d2 = xcc-cs;
                float d3 = xcc-cs + sq*cs;
                float d4 = xcc-cs + sq*cs + ycc-cs;
                float d5 = 2.*(xcc-cs) + sq*cs + ycc-cs;
                float d6 = 2.*(xcc-cs) + 2.*sq*cs + ycc-cs;
                float d7 = 2.*(xcc-cs) + 2.*sq*cs + 2.*(ycc-cs);

                data = sbLineDist(p, p1, p2, thickness); lhasBG *= sbPartition(p, p1, p2);
                float s = step(1., data.x);
                cDist = (d1 + data.y) * s + cDist * (1.-s);
                //
                data = sbLineDist(p, p2, p3, thickness); lhasBG *= sbPartition(p, p2, p3);
                s = step(1., data.x);
                cDist = (d2 + data.y) * s + cDist * (1.-s);
                //
                data = sbLineDist(p, p3, p4, thickness); lhasBG *= sbPartition(p, p3, p4);
                s = step(1., data.x);
                cDist = (d3 + data.y) * s + cDist * (1.-s);
                //
                data = sbLineDist(p, p4, p5, thickness); lhasBG *= sbPartition(p, p4, p5);
                s = step(1., data.x);
                cDist = (d4 + data.y) * s + cDist * (1.-s);
                //
                data = sbLineDist(p, p5, p6, thickness); lhasBG *= sbPartition(p, p5, p6);
                s = step(1., data.x);
                cDist = (d5 + data.y) * s + cDist * (1.-s);
                //
                data = sbLineDist(p, p6, p1, thickness); lhasBG *= sbPartition(p, p6, p1);
                s = step(1., data.x);
                cDist = (d6 + data.y) * s + cDist * (1.-s);

                if(cutBG) hasBG *= lhasBG;

	            if(cDist == -1.) return 0.;

                float maxDist = d7; offset = amod(offset, maxDist); float rdist;
                float exists = 0.;

                for(int i=0; i<repCount; i++)
                {
                    
	                rdist = abs(cDist - amod(offset + repOffset * i, maxDist));
	                rdist = maxDist*.5 - abs(rdist - maxDist*.5);

	                exists += step(rdist, size*.5);
                }
                
                return saturate(exists);
            }


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

            float sbHighlightBox(float2 p)
            {
                float inset1 = .1;
                float inset2 = .22;
                return saturate(
                    sbPolygonHighlight(p, .04, -_Time.y*2., .3, 4.7-inset1, .9-inset1, 3., 0.5, false) + 
                    sbPolygonHighlight(p, .04, -_Time.y*2.-.9*(.5/.9)+.64, .1, 4.7-inset1, .9-inset1, 42., 0.5, false) + 
                    //sbPolygonHighlight(p, .02, -_Time.y*2.5, 19., 4.7-inset2, .9-inset2, 1., 0., false) + 
                    sbPolygonHighlight(p, .04, -_Time.y*2., 100., 4.7, .9, 1., 0., true));
            }

            fixed4 frag(vOut i) : SV_Target
            {
                
                hasBG = 1.;
                float2 p = (i.uv*2.-1.) * float2(_Dimensions.x/_Dimensions.y, 1.);
                float exists = sbHighlightBox(p);
                
                return exists * _InnerHighlightColor + hasBG * (1.-exists) * _NonHighlightColor1;

                /*

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

                return finalCol;*/

            }
            ENDCG
        }
    }
}