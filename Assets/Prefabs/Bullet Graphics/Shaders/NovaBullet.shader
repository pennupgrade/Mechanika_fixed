Shader "Unlit/Bullet/NovaBullet"
{
    Properties
    {
        _LowColor ("Low Color", Color) = (0., .8, 1., 1.)
        _HighColor ("High Color", Color) = (0., .3, 1., 1.)
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

            float4 _LowColor;
            float4 _HighColor;

            fixed4 frag (vOut i) : SV_Target
            {
                float2 p = i.uv*2.-1.;
                p -= amod(p, 0.02);

                float2 polar = toPolar(p);
                
                // Params
                float r = .4;
                
                float stripeSize = .1;
                float stripeRep = .2;
                float2 fadeOutRange = float2(.5, .9);

                // Inner Circle
                float inCircle = step(polar.x, r);

                // Outer Stripe Stuff
                float lr = amod(polar.x - _Time.y, stripeRep) - stripeRep*.5;
                float rId = polar.x - lr; float op = smoothstep(fadeOutRange.y, fadeOutRange.x, rId);
                float isStripe = step(abs(lr), stripeSize*.5) * (1.-inCircle);

                // Composite
                float lowIntensity = inCircle + isStripe*op;

                /// Blue Overlay

                float rOverlay = .2;

                float repCount = 5.;
                float bannerSize = .2;

                float size = TAU / repCount;

                // Inner Circle Overlay
                float inOverlayCircle = step(polar.x, rOverlay);

                // Overlay Banner Stuff
                float ltheta = amod(polar.y, size) - size*.5;
                bannerSize *= pow(polar.x, -1.);
                float isBanner = step(abs(ltheta), bannerSize*.5) * smoothstep(fadeOutRange.y, fadeOutRange.x, polar.x) * (1.-inOverlayCircle);

                // Overlay Composite
                float highIntensity = inOverlayCircle;// + isBanner;

                // Final Composite
                float3 lowCol = lerp(_LowColor, _HighColor, 1.-op) * lowIntensity;
                float3 highCol = _HighColor * highIntensity;
                float3 col = lerp(_HighColor, _LowColor, saturate((polar.x - amod(polar.x, .15))/(.15*3.)));//, highIntensity);

                return float4(col, pow(max(lowIntensity, highIntensity), 3.));
            }
            ENDCG
        }
    }
}
