Shader "Unlit/BulletShaders/OuterGlowBullet"
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
                o.InstanceID = instanceID;

                return o;
            }

            float4 _Color;

            fixed4 frag(vOut i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                i.uv = pixelate(i.uv);

                float falloff = 5.;//10
                float offset = 0.;//0.1
                float partitions = 16.; float thetaLength = 2. * PI / partitions;

                float r = length(i.uv);
                
                float theta = atan2(i.uv.y, i.uv.x) + 2. * PI;
                float rtheta = fmod(theta, thetaLength) - thetaLength * .5;
                float rand = hash11(theta - rtheta); float randLength = sin(_Time.y*4. + 100. * rand+100.*float(i.InstanceID));
                float isLine = step(abs(rtheta - thetaLength * .3 * (rand * 2. - 1.)), max(0., .02 - pow(r - .4, 2.0) / (i.radius * (2. - .7 + rand * 23. + randLength * 4.))));

                float ball = step(r, i.radius);
                float3 col = _Color.rgb *max((1. / pow(falloff * (r - (i.radius - offset)), 2.0)), isLine) + ball;
                col *= smoothstep(2.4, 1.9, r);
                float exists = length(col);

                return float4(col, exists);
            }
            ENDCG
        }
    }
}
