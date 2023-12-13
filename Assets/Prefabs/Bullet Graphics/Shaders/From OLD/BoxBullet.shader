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
                float2 p = i.uv;
                i.radius *= .88;

                //
                float size = i.radius*2.;
                float outlineSize = i.radius*0.2;

                //
                float t = randLinear(_Time.y, float(i.InstanceID), float2(1., .4));
                p = rot(p, t);

                // Inner
                float2 lp = abs(p);
                float innerExists = step(lp.x, size*.5) * step(lp.y, size*.5);
                float outlineExists = innerExists * (1.-(step(lp.x, size*.5-outlineSize)*step(lp.y, size*.5-outlineSize)));
                float4 innerCol = float4(float3(1.,1.,1.) * (1.-outlineExists) + _Color.rgb * outlineExists, innerExists);

                // Outer
                float repSize = i.radius*0.4;
                float stripeSize = i.radius*0.2;
                float2 stripeIntensityRange = float2(i.radius*1.4, i.radius*1.8);

                float sr = max(lp.x, lp.y);
                float lsr = amod(sr+t, repSize) - repSize*.5; float sid = sr - lsr;
                float stripeExists = step(abs(lsr), stripeSize*.5);
                float4 outerCol = float4(_Color.rgb, stripeExists * (1.-innerExists)) * smoothstep(stripeIntensityRange.y, stripeIntensityRange.x, sid);

                // Glow
                float glow = getGlow(sr, i.radius*.7, 0.2);
                float4 emission = float4(_Color.rgb * glow, glow);
                outerCol.a += emission.a;

                return outerCol + (innerCol - outerCol) * innerCol.a;
            }
            ENDCG
        }
    }
}
