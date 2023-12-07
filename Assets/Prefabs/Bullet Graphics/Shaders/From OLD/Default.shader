Shader "Unlit/BulletShaders/Default"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
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

            fixed4 frag(vOut i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                i.uv = snap(i.uv + 2., .04 * .25 * 2.*2.) - 2.;

                //float2 p = i.uv;// float2(abs(i.uv.x), i.uv.y);
                //return float4(1., 1., 1., 0.) * sdPartition(i.uv, float2(1., 0.)) + float4(0., 0., 0., 1.);
                //float sd = sdSpikeBall(float2(.4, .7), 20., p, .3);//sdSpikeBall(float2(.4+.2*sin(_Time.y*1.4), .7 + .1 * cos(_Time.y)), 10., p, .3); // sdRightTri(p, float2(0., 0.), float2(0.3, 0.4)); //sdPolarLine(float2(.1, .2+.1*sin(_Time.y)), 3.14159265 * 1./3., p); 
                //float exists = step(sd, 0.);
                //float3 col = step(sd, -0.05);
                //return float4(col, exists);

                //float r = length(i.uv);

                ////Cut
                //float exists = step(r, i.radius);

                //return a + exists* float4(i.uv.x * .5 + .5, i.uv.y * .5 + .5, 1., 1.);

                
                /*float2 p = i.uv;
                float sd = sdSpikeBall(float2(.4, .7), 20., p, .3, float(i.instanceID));
                float r = length(p);

                float exists = step(sd, 0.);
                float3 col = step(sd, -0.1);

                col += smoothstep(0., .55, r) * float3(0., -1., -1.);*/
                

                
                /*float outline = .9;
                float changeStart = .5;

                float r = length(i.uv);
                float exists = step(r, i.radius);
                float3 col = 1. - step(i.radius * outline, r);
                col += smoothstep(i.radius * changeStart, i.radius * outline, r) * -(1. - float3(.949, .074, .557));*/
                

                /*float outlineDepth = .05;

                float2 p = i.uv;
                float r = length(p);
                float sd = sdSpikeBall(float2(.3+.1, .7+.02), 10., p, 0., 0.);
                float exists = step(sd, 0.);
                float3 col = 1. - step(-outlineDepth, sd);
                col += smoothstep(.1, .45, r) * -(1. - float3(.949, .074, .557));*/

                float falloff = 5.;//10
                float offset = 0.;//0.1
                float partitions = 16.; float thetaLength = 2. * PI / partitions;

                float r = length(i.uv);
                
                float theta = atan2(i.uv.y, i.uv.x) + 2. * PI;
                float rtheta = fmod(theta, thetaLength) - thetaLength * .5;
                float rand = hash11(theta - rtheta); float randLength = sin(_Time.y*4. + 100. * rand+100.*float(i.InstanceID));
                float isLine = step(abs(rtheta - thetaLength * .3 * (rand * 2. - 1.)), max(0., .02 - pow(r - .4, 2.0) / (i.radius * (2. - .7 + rand * 23. + randLength * 4.))));

                float ball = step(r, i.radius);
                float3 col = float3(.949, .074, .557) * max((1. / pow(falloff * (r - (i.radius - offset)), 2.0)), isLine) + ball;
                col *= smoothstep(2.4, 1.9, r);
                float exists = length(col);

                return float4(col, exists);
            }
            ENDCG
        }
    }
}
