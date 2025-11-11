Shader "Universal Render Pipeline/2D/Sprite-Lit-VisibilityCutoff"
{
    Properties
    {
        _MainTex("Base Map", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _VisibilityThreshold("Görünürlük Eşiği (Min Işık)", Range(0.01, 0.5)) = 0.11
        _GlowIntensity ("Ekstra Parlaklık", Range(1, 10)) = 1.0 // <-- YENİ EKLENTİ (Slider)
        
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

            // --- Vertex Shader (Orijinaliyle aynı) ---
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

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);        SAMPLER(sampler_MaskTex);
            TEXTURE2D(_NormalMap);      SAMPLER(sampler_NormalMap); 
            half4 _MainTex_ST;
            half4 _NormalMap_ST; 
            float4 _Color;
            half4 _RendererColor;
            half _VisibilityThreshold;
            half _GlowIntensity; // <-- YENİ EKLENTİ (Değişken)

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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy); 
                o.color = v.color * _Color * _RendererColor;
                return o;
            }

            // --- URP'nin Orijinal Include'unu Kullanıyoruz ---
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            // --- YENİ FRAGMENT SHADER (4 KANALI DİNLEYEN) ---
            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                // 1. TÜM IŞIK KANALLARINI OKU (Önceki düzeltmemiz)
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

                // 2. EN PARLAK IŞIĞI BUL (Önceki düzeltmemiz)
                half4 baseLight = max(light0, max(light1, light2));
                half4 spotLight = light3;
                half4 totalLight = max(baseLight, spotLight); 

                // 3. GÖRÜNÜRLÜK KONTROLÜ (Önceki düzeltmemiz)
                half lightBrightness = max(totalLight.r, max(totalLight.g, totalLight.b));
                if (lightBrightness < _VisibilityThreshold)
                {
                    discard; 
                }

                // --- BURADAN SONRASI SADECE IŞIK YETERLİYSE ÇALIŞIR ---
                
                const half4 mainTexColor = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if (mainTexColor.a < 0.01) { discard; }

                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                const half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv)); 
                SurfaceData2D surfaceData;
                InputData2D inputData;
                InitializeSurfaceData(mainTexColor.rgb, mainTexColor.a, mask, surfaceData); 
                surfaceData.normalTS = normalTS; 
                InitializeInputData(i.uv, i.lightingUV, inputData);
                
                // Orijinal URP hesaplamasını çağır (Normal mapping için bu gerekli)
                half4 finalLitColor = CombinedShapeLightShared(surfaceData, inputData); 

                // <-- YENİ EKLENTİ (Parlaklık Artırma) ---
                // Hesaplanan son rengin RGB'sini (görünen renk)
                // Inspector'daki slider'ımız ile çarpıyoruz.
                finalLitColor.rgb *= _GlowIntensity;
                // --- EKLENTİ BİTTİ ---

                return finalLitColor;
            }
            ENDHLSL
        }
        
        Pass { Name "NormalsRendering" Tags { "LightMode"="NormalsRendering"} HLSLPROGRAM /* ... URP Kodu ... */ ENDHLSL }
        Pass { Name "ForwardUnlit" Tags { "LightMode"="UniversalForward"} HLSLPROGRAM /* ... URP Kodu ... */ ENDHLSL }
    }
    Fallback "Sprites/Default"
}