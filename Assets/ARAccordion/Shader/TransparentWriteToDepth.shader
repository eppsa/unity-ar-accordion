Shader "Custom/Transparent Write To Depth"
{
    Properties
    {
        _Color ("Color (RGBA)", Color) = (1.0, 1.0, 1.0, 0.5)
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode"="ForwardBase" "RenderType"="Transparent" "Queue"="Transparent" }
            LOD 200
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag
                #include "UnityCG.cginc"

                float4 _Color;

                struct Varyings
                {
                    float4 vertex : SV_POSITION;
                };

                Varyings Vert(appdata_base v)
                {
                    Varyings o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    return o;
                }

                float4 Frag(Varyings i) : SV_Target
                {
                    return _Color;
                }

            ENDCG
        }

        Pass
        {
            Tags { "LightMode"="ShadowCaster" "RenderType"="Opaque" "Queue"="Geometry" }

            CGPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag
                #pragma multi_compile_shadowcaster
                #include "UnityCG.cginc"

                struct Varyings
                { 
                    V2F_SHADOW_CASTER;
                };

                Varyings Vert(appdata_base v)
                {
                    Varyings o;
                    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                    return o;
                }

                float4 Frag(Varyings i) : SV_Target
                {
                    SHADOW_CASTER_FRAGMENT(i)
                }

            ENDCG
        }
    }
}