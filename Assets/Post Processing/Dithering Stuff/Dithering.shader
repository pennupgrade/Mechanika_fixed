Shader "Hidden/Custom/Dithering"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        //#include "../../../Graphics/Common.cginc"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        
        float _Interpolation;
        sampler2D _DitherTexture;

        float2 _TimeVariation;
        float4 _UnderDitherColor;

        float _Scale;

        float2 _Speed;

        float _Offset; //could make float3 so other colors leave bfore u know ~ [0, 1] for death

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

            //float2 p = (i.texcoord.xy * 2. - 1.)* float2(16. / 9., 1.); //TODO: uniform in the actual aspect ratio since this is the real screen
            return tex2D(_DitherTexture, i.texcoord);
            float rand = _TimeVariation.y * sin(_Time.y * _TimeVariation.x);
            col.rgb = lerp(col.rgb, _UnderDitherColor.rgb + (col.rgb - _UnderDitherColor.rgb) * step(tex2D(_DitherTexture, (i.texcoord.xy+_Speed*_Time.y)*_Scale)+_Offset+rand, col.rgb), _Interpolation+_Offset*0.6);
            
            return col;
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }
    }
}