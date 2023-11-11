Shader "Unlit/Shadow"
{
    Properties
    {
        _Opacity ("Opacity", Float) = 0.5
        _Color ("Color", Vector) = (0., 0., 0., 1.)
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
            #include "../Common.cginc"

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vOut
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                vOut o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            float _Opacity;
            float4 _Color;

            fixed4 frag(vOut i) : SV_Target
            {
                return _Color * _Opacity * step(length(i.uv) - 1., 0.);
            }
            ENDCG
        }
    }
}
