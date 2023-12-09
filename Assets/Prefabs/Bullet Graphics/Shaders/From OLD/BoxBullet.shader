Shader "Unlit/BulletShaders/BoxBullet"
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

                //
                float size = i.radius*2.;
                float t = _Time.y + 100.*hash11(float(i.InstanceID) * 20.);

                return float4(1., 1., 1., 1.);
            }
            ENDCG
        }
    }
}
