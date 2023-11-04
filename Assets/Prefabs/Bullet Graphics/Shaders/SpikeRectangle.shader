Shader "Unlit/Bullet/SpikeRectangle"
{
    Properties
    {
        _Size ("Size", Float) = 0.5
        _Direction ("Direction", Vector) = (1.,0.,0.,0.)
        _Color ("Color", Color) = (0.,0.,1.,0.)
        _InstanceID ("Instance ID", Float) = 1.
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../../VN/Shaders/Common.hlsl"

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

            float _Size;
            float2 _Direction;
            float4 _Color;
            float _InstanceID;

            fixed4 frag (vOut i) : SV_Target
            {
                //
                float Size = _Size;
                float2 Direction = _Direction;
                float4 Color = _Color;
                i.uv = i.uv*2.-1.;

                //
                float2 up = Direction;
                float2 fo = perp(up);
                float2 p = float2(dot(i.uv, fo), dot(i.uv, up));
                p /= Size;

                //
                float isWhite = 0.;
                float emission = 0.;

                float sd = sdBox(p, float2(1., 2.));
                isWhite += step(sd, 0.);
                emission += step(-Size * .1, sd) * step(sd, 0.);
                emission += step(0., sd) / (70. * sd * sd);
                emission *= smoothstep(1., .8, length(i.uv));

                //
                //emission += step(sdBox(p - float2(0., -.65), float2(.75, .4)), 0.);// *step(0., sdBox(p - float2(0., -.65), float2(.75, .4) - .31));
                //emission += step(sdBox(p - float2(.08, -.3), float2(.24, .3)), 0.);
                //emission += step(sdBox(p, float2(.6, .3)), 0.);
                float inFirstBox = sb(sdBox(p, float2(1., 2.) - .2));
                emission += inFirstBox * step(0., sdBox(p, float2(1. - .3*(sin(40.*p.y+_Time.y*20.)*.5+.5)-.15*(sin(15*p.y+20.)*.5+.5), 2. - .3*(sin(40*p.x+_Time.y*10.)*.5+.5)) - .4));

                // lines criss cross a bit like 2-3
                float isLine = 0.;
                float t = .09;
                float time = .3*_Time.y+_InstanceID*100.; float cr = 2.*.1;
                isLine += sb(sdLine(p, float2(-.5, .64) + cr*toCartesian(sin(time*2.+1.), time+4.), float2(.5, .25) + cr * toCartesian(sin(time * 1. + 2.), -2.*time + 4.), t));
                isLine += sb(sdLine(p, float2(.3, .2) + cr*toCartesian(sin(-time*1.4+4.), time*4.), float2(-.5, -.27) + cr*toCartesian(sin(time*3.), -time*1.5), t));
                isLine += sb(sdLine(p, float2(-.5, -.2) + cr*toCartesian(sin(time*2.3), -time*2.2), float2(.5, -.6 - .15 * 0.), t));
                emission += isLine * inFirstBox;

                //
                /*p.y = abs(p.y);
                float s = .2;
                emission += step(sdBox(p, float2(s, s)), 0.) + step(sdBox(p - float2(0., .4), float2(s, s)), 0.);*/

                //
                isWhite = saturate(isWhite);
                emission = saturate(emission);
                
                float3 col = lerp(isWhite * V.xxx, Color, emission);

                //
                return float4(col, saturate(emission + isWhite));
            }
            ENDCG
        }
    }
}
