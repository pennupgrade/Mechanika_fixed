Shader "Unlit/Bullet/BossLaserParticle"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../../../VN/Shaders/Common.hlsl"

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint VertexID : SV_VertexID;
            };

            struct vOut
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                uint InstanceID : TEXCOORD1;
            };

            vOut vert (vIn v)
            {
                vOut o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.InstanceID = v.VertexID;

                return o;
            }

            float getGlow(float dist)
            {
                return smoothstep(0.2, 0.1, dist)/(max(1., pow((dist+.11+.04)*10., 2.)));
            }

            float getGlow2(float dist)
            {
                return 4.*smoothstep(0.2, 0.1, dist)/(max(1., pow((dist+.11+.04)*15., 2.)));
            }

            float4 renderParticle(float2 p, uint instanceID)
            {
                float instanceRand = hash11(float(instanceID));

                float3 edgeCol = float3(0., 0.7, 0.9);
                float3 insideCol = float3(1., 1., 1.);

                float size = .2*.6*.4;
                float stepSize = size*.14*1.5*0.4;
                float stepCount = 3.*3.;
    
                p = pixelate(p, 0.014-.004);
                float2 polar = toPolar(p);
                
                float rDistort = .2*fsin(polar.y*12.+_Time.y*10.);
                float d = sdHyperCircle(p, size*(1.2+rDistort), 4.);
                float cd = sdHyperCircle(p, size, 4.);
                
                float2 rpolar = polar;
                rpolar.y = amod(rpolar.y, TAU/6.) - TAU/12.; float rand = hash11(polar.y - rpolar.y + instanceRand*10.)*2.-1.;
                float2 lp = toCartesian(rpolar);
                d = min(d, sdLine(lp, toCartesian(float2(1., .5*rand*TAU/12.+.4*sin((2.+instanceRand)*_Time.y+instanceRand*100.))), 0.01*smoothstep(0.05, 0.1, rpolar.x)*smoothstep(0.15, 0.06, rpolar.x)));
    
                float exists = sb(d);
                float distID = saturate((-d - amod(-d, stepSize))/(stepSize*stepCount));
    
                float g = getGlow2(cd);
                float3 emission = 1.5*edgeCol * g;
    
                float3 col = exists*lerp(edgeCol, insideCol, distID) + (1.-exists) * emission;
                return float4(col, max(exists, g*.3));
            }


            fixed4 frag (vOut i) : SV_Target
            {

                i.uv = i.uv*2.-1.;
                i.uv *= .3; i.uv *= 1.1;
                
                return renderParticle(i.uv, i.InstanceID);

            }
            ENDCG
        }
    }
}
