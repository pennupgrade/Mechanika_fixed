Shader "Unlit/BulletShaders/PelletBullet"
{
    Properties
    {
        [HideInInspector] _Color("Color", Color) = (.949, .074, .557, 1.)
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
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "../Common.cginc"

            StructuredBuffer<float2> Positions;
            StructuredBuffer<float> Radiuses;
            StructuredBuffer<float2> Directions;

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vOut
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float radius : TEXCOORD1;
                uint instanceID : TEXCOORD2;
                float2 screenPos : TEXCOORD3;

            };

            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                vOut o;

                o.vertex = mul(UNITY_MATRIX_VP, float4(v.vertex.xy + Positions[instanceID], -1., 1.));
                o.uv = v.uv;
                o.radius = Radiuses[instanceID];
                o.instanceID = instanceID;
                o.screenPos = ComputeScreenPos(o.vertex).xy;

                return o;
            }

            float4 _Color;

            fixed4 frag(vOut i) : SV_Target
            {
                i.screenPos = i.screenPos;

                float3 redCol = float3(.8, .05, .1);// float3(1., .1, .5);// 
                
                float repLen = 0.4;
                float stripeLen = .06;

                float2 stripeDir = float2(1., -1.) / sqrt(2.);// float2(cos(_Time.y), sin(_Time.y));
                float stripeX = fmod(dot(i.screenPos, perp(stripeDir))+_Time.y, repLen) - repLen * .5;
                float sdStripe = abs(stripeX) - stripeLen * .5;
                _Color.rgb = lerp(_Color.rgb, redCol, smoothstep(.04, .0, sdStripe)); //.2

                /*float2 stripeDir = normalize(float2(.2, .8));
                float stripeX = fmod(i.screenPos.x + _Time.y, repLen) - repLen * .5;
                float sdStripe = abs(stripeX) - stripeLen * .5;
                _Color.rgb = lerp(_Color.rgb, float3(0.8, 0.1, 0.1), smoothstep(0., .1, sdStripe));*/

                float o = randTrig(_Time.y, float(i.instanceID)*.01, float2(2.5, 1.));
                float2 fo = float2(cos(o), sin(o));
                float2 up = float2(cos(o + PI * .5), sin(o + PI * .5));

                float2 p = i.uv;
                p = float2(dot(p, fo), dot(p, up));
                p = pixelate(p, 1.);
                p = abs(p);
                
                float thick = i.radius;
                
                float cutOff = min(thick, max(0., i.radius*3. - p.x*p.x)); //make the actual zun color falloff
                float sd = sqrt(pow(max(0., p.y - cutOff), 2.0) + pow(max(0., p.x - sqrt(i.radius*3.)), 2.0));
                sd += step(sd, 0.) * (p.y - cutOff);
                //float sd = length(p * float2(1., 2.2)) - i.radius;

                float stp = 2. * smoothstep(-i.radius * .8, i.radius * .1, sd);
                float3 col = step(stp, 1.) * _Color * stp + (1. - step(stp, 1.)) * lerp(_Color, 1.5*_Color/*.9*float3(1., 1., 1.)*/, stp - 1.);

                col *= step(sd, 0.);
                float exists = step(sd, 0.);

                //col *= 1. - step(sd, -i.radius * .05);
                //col += _Color * pow(max(1. - sd, 0.), 5.0) * step(0., sd);
                col += _Color * min(1, 1. / pow(10. * sd, 3.0));// *step(0., sd);

                return float4(col, max(exists, length(col)));
            }
            ENDCG
        }
    }
}
