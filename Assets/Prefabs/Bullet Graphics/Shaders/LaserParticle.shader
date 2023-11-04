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
            };

            struct vOut
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            vOut vert (vIn v)
            {
                vOut o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            float sdHyperCircle(float2 p, float r, float k)
            {
                return pow(pow(p.x, k) + pow(p.y, k), 1./k) - r;
            }

            float getGlow(float dist)
            {
                return smoothstep(0.2, 0.1, dist)/(max(1., pow((dist+.11+.04)*10., 2.)));
            }

            float4 renderParticle(float2 p)
            {
                float3 edgeCol = float3(0., 0.7, 0.9);
                float3 insideCol = float3(1., 1., 1.);

                float size = .2*.6*.4;
                float stepSize = size*.14*1.5;
                float stepCount = 3.;
    
                p = pixelate(p, 0.014);
                float d = sdHyperCircle(p, size*1.2, 4.);
                float cd = sdHyperCircle(p, size, 4.);
    
                float exists = sb(d);
                float distID = saturate((-d - amod(-d, stepSize))/(stepSize*stepCount));
    
                float g = getGlow(cd);
                float3 emission = 1.5*edgeCol * g;
    
                float3 col = exists*lerp(edgeCol, insideCol, distID) + (1.-exists) * emission;
                return float4(col, max(exists, g*.3));
            }


            fixed4 frag (vOut i) : SV_Target
            {

                i.uv = i.uv*2.-1.;
                i.uv *= .3;
                
                return renderParticle(i.uv);

            }
            ENDCG
        }
    }
}
