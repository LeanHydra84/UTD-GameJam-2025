Shader "PostProcess/ASCIIEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CharacterSize ("Character Size", Range(1, 32)) = 8
        _Contrast ("Contrast", Range(0.1, 3.0)) = 1.0
        _Brightness ("Brightness", Range(-1.0, 1.0)) = 0.0
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 1)
        _TextColor ("Text Color", Color) = (1, 1, 1, 1)
        _UseOriginalColors ("Use Original Colors", Range(0, 1)) = 1
        _ColorIntensity ("Color Intensity", Range(0, 2)) = 1.0
        _BackgroundBlend ("Background Blend", Range(0, 1)) = 0.2
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        
        ZWrite Off
        ZTest Always
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _CharacterSize;
            float _Contrast;
            float _Brightness;
            fixed4 _BackgroundColor;
            fixed4 _TextColor;
            float _UseOriginalColors;
            float _ColorIntensity;
            float _BackgroundBlend;

            // come back and make real character pattern jakle
            float getCharacterPattern(int charIndex, float2 localUV)
            {
                float2 p = localUV * 2.0 - 1.0; // -1 to 1 range
                
                if (charIndex == 0) // blankspace
                {
                    return 0.0;
                }
                else if (charIndex == 1) // .
                {
                    return step(length(p), 0.3) * step(0.5, localUV.y);
                }
                else if (charIndex == 2) // :
                {
                    float dot1 = step(length(p - float2(0, 0.4)), 0.2);
                    float dot2 = step(length(p - float2(0, -0.4)), 0.2);
                    return max(dot1, dot2);
                }
                else if (charIndex == 3) // -
                {
                    return step(abs(p.y), 0.15) * step(abs(p.x), 0.7);
                }
                else if (charIndex == 4) // =
                {
                    float line1 = step(abs(p.y - 0.3), 0.1) * step(abs(p.x), 0.7);
                    float line2 = step(abs(p.y + 0.3), 0.1) * step(abs(p.x), 0.7);
                    return max(line1, line2);
                }
                else if (charIndex == 5) // +
                {
                    float horizontal = step(abs(p.y), 0.15) * step(abs(p.x), 0.7);
                    float vertical = step(abs(p.x), 0.15) * step(abs(p.y), 0.7);
                    return max(horizontal, vertical);
                }
                else if (charIndex == 6) // *
                {
                    float horizontal = step(abs(p.y), 0.1) * step(abs(p.x), 0.6);
                    float vertical = step(abs(p.x), 0.1) * step(abs(p.y), 0.6);
                    float diagonal1 = step(abs(p.x - p.y), 0.1) * step(length(p), 0.7);
                    float diagonal2 = step(abs(p.x + p.y), 0.1) * step(length(p), 0.7);
                    return max(max(horizontal, vertical), max(diagonal1, diagonal2));
                }
                else if (charIndex == 7) // #
                {
                    float h1 = step(abs(p.y - 0.4), 0.08) * step(abs(p.x), 0.8);
                    float h2 = step(abs(p.y + 0.4), 0.08) * step(abs(p.x), 0.8);
                    float v1 = step(abs(p.x - 0.3), 0.08) * step(abs(p.y), 0.8);
                    float v2 = step(abs(p.x + 0.3), 0.08) * step(abs(p.y), 0.8);
                    return max(max(h1, h2), max(v1, v2));
                }
                else if (charIndex == 8) // @
                {
                    float outer = step(0.3, length(p)) * step(length(p), 0.8);
                    float inner = step(length(p - float2(0.2, 0)), 0.3);
                    return max(outer, inner);
                }
                
                return 0.0;
            }

            // convert brightness to character index
            int brightnessToChar(float brightness)
            {
                brightness = saturate(brightness);
                
                if (brightness < 0.1) return 0; // space
                else if (brightness < 0.2) return 1; // .
                else if (brightness < 0.3) return 2; // :
                else if (brightness < 0.4) return 3; // -
                else if (brightness < 0.5) return 4; // =
                else if (brightness < 0.6) return 5; // +
                else if (brightness < 0.8) return 6; // *
                else if (brightness < 0.9) return 7; // #
                else return 8; // @
            }

            fixed4 frag (v2f_img i) : SV_Target
            {
                // screen resolution
                float2 screenRes = _ScreenParams.xy;
                
                // character grid position
                float2 charCoord = floor(i.uv * screenRes / _CharacterSize);
                float2 charUV = frac(i.uv * screenRes / _CharacterSize);
                
                // sample cell
                float2 cellCenter = (charCoord + 0.5) * _CharacterSize / screenRes;
                fixed4 originalColor = tex2D(_MainTex, cellCenter);
                
                // calculate brightness
                float brightness = dot(originalColor.rgb, float3(0.299, 0.587, 0.114));
                brightness = saturate((brightness + _Brightness) * _Contrast);
                
                // character index based on brightness
                int charIndex = brightnessToChar(brightness);
                
                // pixel value
                float charPixel = getCharacterPattern(charIndex, charUV);
                
                // Color calculation
                fixed4 finalColor;
                
                if (_UseOriginalColors > 0.5)
                {
                    // original colors
                    fixed4 textColor = originalColor * _ColorIntensity;
                    fixed4 bgColor = lerp(_BackgroundColor, originalColor, _BackgroundBlend);
                    finalColor = lerp(bgColor, textColor, charPixel);
                }
                else
                {
                    // preset colors
                    finalColor = lerp(_BackgroundColor, _TextColor, charPixel);
                }
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}