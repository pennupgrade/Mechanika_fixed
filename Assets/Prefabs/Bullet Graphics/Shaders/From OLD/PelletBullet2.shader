Shader "Unlit/BulletShaders/PelletBullet2"
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
            #include "../../../../VN/Shaders/Common.hlsl"
            #include "BulletProps.hlsl"

            /*StructuredBuffer<float2> Positions;
            StructuredBuffer<float> Radiuses;
            StructuredBuffer<float2> Directions;

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };*/

            struct vOut
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float radius : TEXCOORD1;
                uint InstanceID : TEXCOORD2;
                float2 screenPos : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                vOut o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.radius = UNITY_ACCESS_INSTANCED_PROP(bulletProps, Radiuses);
                o.InstanceID = instanceID % 15;
                o.screenPos = ComputeScreenPos(o.vertex).xy;

                return o;
            }

            float4 _Color;

            fixed4 frag(vOut i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                i.screenPos = i.screenPos;

                float3 redCol = float3(1., .1, .5);// float3(.8, .05, .1);
                
                float repLen = 0.4;
                float stripeLen = .06;

                float2 stripeDir = float2(1., -1.) / sqrt(2.);// float2(cos(_Time.y), sin(_Time.y));
                float stripeX = fmod(dot(i.screenPos, perp(stripeDir))+_Time.y, repLen) - repLen * .5;
                float sdStripe = abs(stripeX) - stripeLen * .5;
                _Color.rgb = lerp(_Color.rgb, redCol, smoothstep(.04, .0, sdStripe)); //.2

                float o = randLinear(_Time.y+8., float(i.InstanceID)*.01, float2(2.5+6., 1.+2.));
                float2 fo = float2(cos(o), sin(o));
                float2 up = float2(cos(o + PI * .5), sin(o + PI * .5));

                float2 p = i.uv;
                p = float2(dot(p, fo), dot(p, up));
                p = pixelate(p, 1.);
                p = abs(p);
                
                float sd = length(p * float2(1., 2.2)) - i.radius*3.;
                float exists = step(sd, 0.);
                float3 col = exists;

                col *= _Color.rgb * smoothstep(-i.radius * 1.7, -i.radius * .4, sd);
                //col += step(-i.radius * .4, sd);
                col += step(0., sd) * _Color.rgb * 1. / pow(2. * sd, 2.);

                return float4(col, max(exists, length(col)));
            }
            ENDCG
        }
    }
}
