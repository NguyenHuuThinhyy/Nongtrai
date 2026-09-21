Shader "Farm/Depth Font"
{
    Properties { _MainTex("Font", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            Varyings vert(Attributes v) { Varyings o; o.positionCS = TransformObjectToHClip(v.positionOS.xyz); o.uv=v.uv; o.color=v.color; return o; }
            half4 frag(Varyings i):SV_Target { half a=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a; return half4(i.color.rgb,i.color.a*a); }
            ENDHLSL
        }
    }
}
