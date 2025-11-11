Shader "Custom/PulseTracer"
{
    Properties
    {
        _TintColor ("Mermi Rengi (Sarı)", Color) = (1, 0.8, 0, 1)
        _GlowIntensity ("Parlaklık Yoğunluğu (Bloom için)", Range(1, 10)) = 1.0
        // _Progress (0-1 arası) adında bir değişkeni C# kodundan alacağız
        _Progress ("İlerleme (C# tarafından kontrol edilir)", Range(0, 1)) = 1.0
        _FadeOutPower ("Sönme Gücü", Range(1, 10)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR; 
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            fixed4 _TintColor;
            float _GlowIntensity;
            float _Progress; // 0'dan (yeni) 1'e (eski) giden C# değişkeni
            float _FadeOutPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // LineRenderer'ın kendi beyaz rengini ve bizim rengimizi birleştir
                o.color = v.color * _TintColor; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;

                // _Progress C# tarafından 0'dan 1'e doğru artırılacak.
                // Biz (1.0 - _Progress) kullanarak 1'den 0'a doğru sönen bir alpha yapıyoruz.
                float alpha = pow(1.0 - _Progress, _FadeOutPower);
                
                col.a *= alpha; // Ana alpha'yı uygula
                col.rgb *= _GlowIntensity; // Parlaklığı uygula

                return col;
            }
            ENDCG
        }
    }
}