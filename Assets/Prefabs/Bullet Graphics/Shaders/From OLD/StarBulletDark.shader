Shader "Unlit/BulletShaders/StarBulletDark"
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

                i.uv = pixelate(i.uv, .5);
                
                float outlineDepth = .05;

                float2 p = i.uv;
                float r = length(p);
                float sd = sdSpikeBall(float2(i.radius, (i.radius/.4)*.72), 10., p, 0., float(i.InstanceID), _Time.y);
                float exists = step(sd, 0.);
                float3 col = 1. -step(-outlineDepth, sd);
                col += smoothstep(.1*i.radius/.4, .45*i.radius/.4, r) * -(1. - _Color.rgb);

                return float4(col, exists);
            }
            ENDCG
        }
    }
}
