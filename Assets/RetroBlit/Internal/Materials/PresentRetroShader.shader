﻿/* Custom shaders should be based on PresentBasicShader.shader, not this shader */

Shader "Unlit/PresentRetroShader"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Lowest target for greatest cross-platform compatiblilty
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _SystemTexture;
            sampler2D _PixelTexture;
            float2 _PixelTextureSize;
            float2 _PresentSize;

            float _SampleFactor;

            float _ScanlineIntensity;
            float _DesaturationIntensity;
            float _CurvatureIntensity;

            float3 _ColorFade;
            float _ColorFadeIntensity;

            float3 _ColorTint;
            float _ColorTintIntensity;

            float _NegativeIntensity;

            float _PixelateIntensity;

            /* Custom shaders should be based on PresentBasicShader.shader, not this shader */

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(v.vertex.xy, 0, 1));
                o.uv = v.uv;

                /* Custom shaders should be based on PresentBasicShader.shader, not this shader */

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                /* Custom shaders should be based on PresentBasicShader.shader, not this shader */

                float PI = 3.14159265;
                float2 origUV = i.uv;

                // Curvature
                float curveHorizontalScale = 0.1 * (_PixelTextureSize.y / _PixelTextureSize.x);
                float curveVerticalScale = 0.1 * (_PixelTextureSize.x / _PixelTextureSize.y);
                float2 curve = float2(curveHorizontalScale * _CurvatureIntensity, curveVerticalScale * _CurvatureIntensity);
                i.uv.x += (sin(origUV.y * PI) * curve.x) * (0.5 - origUV.x);
                i.uv.y += (sin(origUV.x * PI) * curve.y) * (0.5 - origUV.y);

                // Pixelate
                _PixelateIntensity = 1.0 + (_PixelateIntensity * 100 * 30);
                float aspectRatio = _PixelTextureSize.x / _PixelTextureSize.y;
                float scaleUp = 100;
                i.uv.x = (floor(((i.uv.x * _PixelTextureSize.x * scaleUp) + (_PixelateIntensity / 0.75)) / (_PixelateIntensity)) * _PixelateIntensity) / (_PixelTextureSize.x * scaleUp);
                i.uv.y = (floor(((i.uv.y * _PixelTextureSize.y * scaleUp) + (_PixelateIntensity / 0.75)) / (_PixelateIntensity)) * _PixelateIntensity) / (_PixelTextureSize.y * scaleUp);

                /* Here we sample neighbouring pixels to get some pixel smoothing when the RetroBlit.DisplaySize
                   doesn't divide evenly into the native window resolution. */
                float2 pixelSize = float2(1.0 / _PixelTextureSize.x, 1.0 / _PixelTextureSize.y);
                pixelSize *= _SampleFactor;

                float4 leftColor = tex2D(_PixelTexture, float2(i.uv.x - pixelSize.x, i.uv.y)).rgba;
                float4 rightColor = tex2D(_PixelTexture, float2(i.uv.x + pixelSize.x, i.uv.y)).rgba;
                float4 topColor = tex2D(_PixelTexture, float2(i.uv.x , i.uv.y + pixelSize.y)).rgba;
                float4 bottomColor = tex2D(_PixelTexture, float2(i.uv.x, i.uv.y - pixelSize.y)).rgba;

                float4 color = (leftColor + rightColor + topColor + bottomColor) / 4.0;

                // Desaturate
                float4 scaledColor = color * float4(0.3, 0.59, 0.11, 1.0);
                float luminance = scaledColor.r + scaledColor.g + scaledColor.b;
                float desatColor = float3(luminance, luminance, luminance);
                color = lerp(color, desatColor, _DesaturationIntensity);

                // No noise or scanlines, to save on extra sampling

                // Color Fade
                color = ((1.0 - _ColorFadeIntensity) * color) + (_ColorFadeIntensity * float4(_ColorFade, 1));

                // Color Tint
                color *= ((1.0 - _ColorTintIntensity) * float4(1, 1, 1, 1)) + (_ColorTintIntensity * float4(_ColorTint, 1));

                // Negative
                color = ((1.0 - _NegativeIntensity) * color) + (_NegativeIntensity * float4(1.0 - color.r, 1.0 - color.g, 1.0 - color.b, 1.0));

                /* Custom shaders should be based on PresentBasicShader.shader, not this shader */

                return color;
            }
            ENDCG
        }
    }
}
