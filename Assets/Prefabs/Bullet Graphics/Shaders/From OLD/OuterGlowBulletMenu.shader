//Shader "Unlit/BulletShaders/OuterGlowBulletMenu" //will have cool stripes or something
//{
//    Properties
//    {
//        [HideInInspector] _Color("Color", Color) = (.949, .074, .557, 1.)
//    }
//        SubShader
//    {
//        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
//        LOD 100
//        Blend SrcAlpha OneMinusSrcAlpha
//        ZWrite Off
//        Cull Off
//
//        Pass
//        {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #pragma multi_compile_instancing
//
//            #include "UnityCG.cginc"
//            #include "../Common.cginc"
//
//            StructuredBuffer<float2> Positions;
//            StructuredBuffer<float> Radiuses;
//            StructuredBuffer<float2> Directions;
//
//            struct vIn
//            {
//                float4 vertex : POSITION;
//                float2 uv : TEXCOORD0;
//            };
//
//            struct vOut
//            {
//                float4 vertex : SV_POSITION;
//                float2 uv : TEXCOORD0;
//                float radius : TEXCOORD1;
//                uint instanceID : TEXCOORD2;
//                float2 screenPos : TEXCOORD3;
//            };
//
//            vOut vert(vIn v, uint instanceID : SV_InstanceID)
//            {
//                vOut o;
//
//                o.vertex = mul(UNITY_MATRIX_VP, float4(v.vertex.xy + Positions[instanceID], -1., 1.));
//                o.uv = v.uv;
//                o.radius = Radiuses[instanceID];
//                o.instanceID = instanceID;
//                o.screenPos = ComputeScreenPos(o.vertex).xy;
//
//                return o;
//            }
//
//            float4 _Color;
//            float _Aspect;
//
//            fixed4 frag(vOut i) : SV_Target
//            {
//                i.uv = pixelate(i.uv);
//
//                //
//                float partitionSize = 2.0 * .25;
//                float stripeSize = 1.0 * .25;
//                float edgeThickness = .1;
//
//                float3 stripeCol1 = float3(.2, .2, .7);
//                float3 stripeCol2 = float3(1., 1., 1.);
//
//                i.screenPos = pixelate((i.screenPos * 2. - 1.) * float2(16. / 9., 1.), .1); float o = fmod(atanP(i.screenPos) + _Time.y * .2, 2. * PI);
//                float sr = length(i.screenPos) + o * partitionSize / (2. * PI);
//                sr = length(float2(16. / 9., 1.)) - sr;
//                sr += _Time.y;
//                float lsr = abs(fmod(sr, partitionSize) - partitionSize * .5);
//                float interpolation = smoothstep((stripeSize + edgeThickness) * .5, (stripeSize - edgeThickness) * .5, lsr);
//
//                float id = (fmod(sr, partitionSize * 2.) - fmod(sr, partitionSize)) / partitionSize; //only 0, 1 so simple not-modular method
//                id = lerp(id, .5, pow(abs(o - PI) / PI, 2.0));
//
//                float4 stripeCol = float4(stripeCol1 + id * (stripeCol2 - stripeCol1), 1.);
//
//                _Color = lerp(_Color, stripeCol, interpolation);
//
//                //
//                float falloff = 5.;//10
//                float offset = 0.;//0.1
//                float partitions = 16.; float thetaLength = 2. * PI / partitions;
//
//                float r = length(i.uv);
//
//                float theta = atan2(i.uv.y, i.uv.x) + 2. * PI;
//                float rtheta = fmod(theta, thetaLength) - thetaLength * .5;
//                float rand = hash11(theta - rtheta); float randLength = sin(_Time.y * 4. + 100. * rand + 100. * float(i.instanceID));
//                float isLine = step(abs(rtheta - thetaLength * .3 * (rand * 2. - 1.)), max(0., .02 - pow(r - .4, 2.0) / (i.radius * (2. - .7 + rand * 23. + randLength * 4.))));
//
//                float ball = step(r, i.radius);
//                float3 col = _Color.rgb * max((1. / pow(falloff * (r - (i.radius - offset)), 2.0)), isLine) + ball;
//                col *= smoothstep(2.4, 1.9, r);
//                float exists = length(col);
//
//                return float4(col, exists);
//            }
//            ENDCG
//        }
//    }
//}



// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/BulletShaders/OuterGlowBulletMenu" //will have cool stripes or something
{
    Properties
    {
        [HideInInspector] _Color("Color", Color) = (.949, .074, .557, 1.)

        [PerRendererData] Positions ("Positions", Vector) = (0.,0.,0.,0.)
        [PerRendererData] Radiuses ("Radiuses", Float) = 1.
        [PerRendererData] Directions ("Directions", Vector) = (0.,0.,0.,0.)
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

            /*UNITY_INSTANCING_BUFFER_START(BulletProperties)
                UNITY_DEFINE_INSTANCED_PROP(float4, Positions)
                UNITY_DEFINE_INSTANCED_PROP(float, Radiuses)
                UNITY_DEFINE_INSTANCED_PROP(float4, Directions)
            UNITY_INSTANCING_BUFFER_END(bulletProps)

            struct vIn
            {

                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID

            };*/

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
                float2 screenPos : TEXCOORD3;
                uint InstanceID : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                vOut o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);// mul(UNITY_MATRIX_VP, float4(v.vertex.xy + UNITY_ACCESS_INSTANCED_PROP(bulletProps, Positions).xy, -1., 1.));
                o.uv = v.uv;
                o.radius = UNITY_ACCESS_INSTANCED_PROP(bulletProps, Radiuses);
                o.InstanceID = instanceID;
                o.screenPos = ComputeScreenPos(o.vertex).xy;

                return o;
            }

            float4 _Color;
            float _Aspect;

            fixed4 frag(vOut i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                i.uv = pixelate(i.uv);

                //
                float partitionSize = 2.0*.25;
                float stripeSize = 1.0*.25;
                float edgeThickness = .1;

                float3 stripeCol1 = float3(.2, .2, .7);
                float3 stripeCol2 = float3(1., 1., 1.);

                i.screenPos = pixelate((i.screenPos * 2. - 1.) * float2(16. / 9., 1.), .1); float o = fmod(atanP(i.screenPos) + _Time.y*.2, 2.*PI);
                float sr = length(i.screenPos) + o * partitionSize / (2. * PI);
                sr = length(float2(16./9., 1.)) - sr;
                sr += _Time.y;
                float lsr = abs(fmod(sr, partitionSize) - partitionSize * .5);
                float interpolation = smoothstep((stripeSize + edgeThickness) * .5, (stripeSize - edgeThickness) * .5, lsr);

                float id = (fmod(sr, partitionSize * 2.) - fmod(sr, partitionSize)) / partitionSize; //only 0, 1 so simple not-modular method
                id = lerp(id, .5, pow(abs(o - PI) / PI, 2.0));

                float4 stripeCol = float4(stripeCol1 + id * (stripeCol2 - stripeCol1), 1.);

                _Color = lerp(_Color, stripeCol, interpolation);
                
                //
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
