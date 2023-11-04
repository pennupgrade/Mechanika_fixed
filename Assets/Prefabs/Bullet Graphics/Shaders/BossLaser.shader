Shader "Unlit/Bullet/BossLaser"
{
    Properties
    {
        //Should be more abstracted
        _LaserScale ("Laser Scale", Float) = 3.

        _Speed ("Speed", Float) = 1.
        _Amplitude ("Amplitude", Float) = 1.

        [HideInInspector] _LaserXScale ("Laser X Scale", Float) = 1.
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
            #include "../../../VN/Shaders/Common.hlsl"

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

            float _LaserScale;

            float _Speed;
            float _Amplitude;

            float _LaserXScale;

            float getGlow(float dist)
            {
                return smoothstep(0.25, 0.1, dist)/(max(1., pow((dist+.11+.04)*10., 2.)));
            }

            float sampleDistort(float x, float t)
            {
                t *= 4.;

                //distort amplitudes
                float amp1 = .5+sin(t*4.)*.1;
                float amp2 = .2+sin(t*2.2)*.08;
                float amp3 = .5+sin(t*5.)*.03; //1
                float ampT = .3+sin(t*2.8)*.05;

                return ampT*(amp3*fsin(x*3.-t*5.) + amp1*pow(fsin(x*6.-t*18. + 2.), 2.) + amp2*fsin(x*1.7 - t*30.));
            }

            float4 renderLaser(float2 p)
            {
                float t = _Time.y;

                p -= amod(p, .01);

                float3 edgeCol = float3(0., 0.7, 0.9);
                float3 insideCol = float3(1., 1., 1.);

                float laserHeight = -.5 + .8 * sqrt(saturate((t - 4.)/(4.3-4.))) + fsin(t*4.4)*.08; //-.5 to oscillate .3 and .5
                float laserStepWidth = .03;
                float laserEndStepCount = 3.;

                //hehe unusual polar
                float r = .5;
                float2 polar = toPolar(p);
                polar.y = abs(polar.y - r);
                
                //Laser body
                p.y = abs(p.y);
                float distort = sampleDistort(p.x, t);
                p.y *= (1. - distort);
                float isLaser = step(p.y, laserHeight*.5);
                
                //Laser color edges
                float distEdge = laserHeight*.5 - p.y;
                float distId = distEdge - amod(distEdge, laserStepWidth);
                float interpo = 1.-saturate((distId)/(laserEndStepCount*laserStepWidth));
                
                //Dithering
                float ditherScale = .01;
                float2 dp = p + float2(t*3., 0.);
                float2 ditherId = dp - amod(dp, ditherScale) + ditherScale*.5;
                float dither = step(ditherScale*.5, abs(amod(ditherId.x + ditherId.y, ditherScale*2.)-ditherScale));
                float ditherInter = saturate(.6*((abs(dither*ditherId.y)-ditherScale*.5)/ditherScale-3.));

                //Glow
                //p = pixelate(p, 0.01);
                float g = getGlow((p.y - laserHeight*.5)/(1.-distort));
                float3 emission = edgeCol * g;
                
                //Compositing
                float3 laserBodyCol = insideCol;
                float3 col = isLaser * lerp(laserBodyCol, edgeCol, interpo) + (1.-isLaser) * emission;
                
                //offset mark vertical part of laser body meaning edge overrides
                return float4(col, max(g, isLaser));
                
            }


            fixed4 frag (vOut i) : SV_Target
            {

                float laserHeightSample = 1./_LaserScale;

                i.uv.x *= _LaserXScale;
                i.uv *= laserHeightSample;
                
                return renderLaser(i.uv);

            }
            ENDCG
        }
    }
}
