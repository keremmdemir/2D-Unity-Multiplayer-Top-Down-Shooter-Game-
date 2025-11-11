Shader "Custom/Line-Lit-Visibility-Tracer"
{
    Properties
    {
        _Color ("Mermi Rengi (Sarı)", Color) = (1, 0.8, 0, 1)
        _GlowIntensity ("Parlaklık Yoğunluğu (Bloom için)", Range(1, 10)) = 1.0
        _Progress ("İlerleme (C# tarafından kontrol edilir)", Range(0, 1)) = 0.0
        _FadeOutPower ("Sönme Gücü", Range(1, 10)) = 2.0
        _VisibilityThreshold("Görünürlük Eşiği (Min Işık)", Range(0.01, 0.5)) = 0.11 

        [HideInInspector] _MainTex("Base Map", 2D) = "white" {}
        [HideInInspector] _MaskTex("Mask", 2D) = "white" {}
        [HideInInspector] _NormalMap("Normal Map", 2D) = "bump" {}
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "Lit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
            #pragma multi_compile _ DEBUG_DISPLAY

            // ---- Vertex Shader (Orijinaliyle aynı) ----
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                half4 color         : COLOR;
                float2 uv           : TEXCOORD0;
                half2 lightingUV    : TEXCOORD1; 
                #if defined(DEBUG_DISPLAY)
                float3 positionWS   : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            float4 _Color;
            half4 _RendererColor;
            half _VisibilityThreshold;
            float _GlowIntensity;
            float _Progress; 
            float _FadeOutPower;

            #if USE_SHAPE_LIGHT_TYPE_0
                SHAPE_LIGHT(0)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_1
                SHAPE_LIGHT(1)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_2
                SHAPE_LIGHT(2)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_3
                SHAPE_LIGHT(3)
            #endif

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = float2(0,0);
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy); 
                o.color = v.color * _Color * _RendererColor; 
                return o;
            }

            // ---- FRAGMENT SHADER (4 KANAL DİNLEYEN VE FADE YAPAN) ----
            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                // 1. TÜM IŞIK KANALLARINI OKU
                half4 light0 = 0, light1 = 0, light2 = 0, light3 = 0;
                #if USE_SHAPE_LIGHT_TYPE_0
                    light0 = SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, i.lightingUV);
                #endif
                #if USE_SHAPE_LIGHT_TYPE_1
                    light1 = SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, i.lightingUV);
                #endif
                #if USE_SHAPE_LIGHT_TYPE_2
                    light2 = SAMPLE_TEXTURE2D(_ShapeLightTexture2, sampler_ShapeLightTexture2, i.lightingUV);
                #endif
                #if USE_SHAPE_LIGHT_TYPE_3
                    light3 = SAMPLE_TEXTURE2D(_ShapeLightTexture3, sampler_ShapeLightTexture3, i.lightingUV);
                #endif

                // 2. EN PARLAK IŞIĞI BUL
                half4 baseLight = max(light0, max(light1, light2)); 
                half4 spotLight = light3; 
                half4 totalLight = max(baseLight, spotLight); 
                half lightBrightness = max(totalLight.r, max(totalLight.g, totalLight.b));

                // 3. GÖRÜNÜRLÜK KONTROLÜ (FOV)
                if (lightBrightness < _VisibilityThreshold)
                {
                    discard; 
                }

                // --- BURADAN SONRASI SADECE IŞIK YETERLİYSE ÇALIŞIR ---

                // 4. TRACER FADE HESAPLAMASI (PulseTracer'dan)
                half fadeAlpha = pow(1.0 - _Progress, _FadeOutPower);
                
                if (fadeAlpha < 0.01)
                {
                    discard;
                }

                // 5. TEMEL RENK & AYDINLATMA
                half4 finalColor = i.color;
                finalColor.a *= fadeAlpha;

                // 6. AYDINLATMA
                // LineRenderer'lar URP'nin tam aydınlatma modelini kullanamaz.
                // Biz sadece en parlak ışıkla (totalLight) çarparız.
                finalColor.rgb *= totalLight.rgb; 
                finalColor.rgb *= _GlowIntensity;

                return finalColor;
            }
            ENDHLSL
        }
    }
    Fallback "Sprites/Default"
}