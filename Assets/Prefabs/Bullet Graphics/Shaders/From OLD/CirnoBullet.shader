Shader "Unlit/BulletShaders/CirnoBullet"
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
                float2 direction : TEXCOORD3;

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
                o.direction = UNITY_ACCESS_INSTANCED_PROP(bulletProps, Directions).xy;

                return o;
            }

            float4 _Color;

            fixed4 frag(vOut i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float2 fo = i.direction;
                float2 up = perp(fo);

                float2 p = i.uv;
                p = float2(dot(p, fo), dot(p, up));
                p.y *= 2.3;

                p = pixelate(p, 1.);

                float sd = sdSpikeBall(float2(0.3, 0.36), 8., p, .15, i.InstanceID, 0.);
                
                float exists = step(sd, .0);

                float3 col = exists;// step(sd, -.05);
                col += -1. * (1.-_Color.rgb) * smoothstep(-.4, -.05, sd);
                //col = max(0., col);
                col += _Color.rgb * min(10., 1. / pow(4.0 * sd, 2.0) * step(0., sd));

                //float3 col = _Color.rgb;

                //Specular
                //float3 spec = float3(.1, .2, .00);
                //col += 1. / pow(27.*(length(p - spec.xy) - spec.z), 3.0);

                //Highlight
                //col *= step(sd, 0.);

                return float4(col, length(max(0., col)));
            }
            ENDCG
        }
    }
}
