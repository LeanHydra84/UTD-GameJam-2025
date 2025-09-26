Shader "Fullscreen/TextRenderShader"
{
    Properties
    {
        _TextAtlas ("Atlas", 2D) = "white" { }
        [HideInInspector] _Width ("Width", Int) = 50
        [HideInInspector] _Height ("Height", Int) = 100
        _Padding ("Padding", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            Name "FullscreenExample"
            ZWrite Off
            Cull Off
            Blend Off // Or specify a blend mode if needed
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            struct Glyph
            {
                float x, y, width, height;
                float scale;
            };

            sampler2D _TextAtlas;
            
            StructuredBuffer<Glyph> _glyphBuffer;
            StructuredBuffer<int> _characterBuffer;

            uint _Width;
            uint _Height;

            float _Padding;

            float4 CalculateRect(uint2 cell, float2 scale)
            {
                float2 bottomleft = cell * scale;
                return float4(bottomleft.xy, scale.xy);
            }

            // int quick_calc_time_index(float2 uv)
            // {
            //     float val = (int)(_Time.w + uv.x + uv.y * 5) % (int)(glyphCount - 1);
            //     return (int)clamp(val, 0, glyphCount - 1);
            // }

            bool is_inside_scale(float2 uv, float2 scale)
            {
                if (uv.x < 0 || uv.y < 0) return false;
                if (uv.x > scale.x || uv.y > scale.y) return false;
                return true;
            }

            float2 get_glyphcoord(float4 glyphdata, float2 relative01)
            {
                float scaler;
                if (glyphdata.z > glyphdata.w)
                {
                    scaler = glyphdata.z;
                    float2 scaled = relative01 * scaler;
                    float wScaleDif = (glyphdata.w * relative01.y) - scaled.y;
                    scaled.y += wScaleDif / 2;
                    if (!is_inside_scale(scaled, glyphdata.zw))
                        return float2(0, 0);
                    return scaled + glyphdata.xy;
                }
                else
                {
                    scaler = glyphdata.w;
                    float2 scaled = relative01 * scaler;
                    float wScaleDif = (glyphdata.z * relative01.x) - scaled.x;
                    scaled.x += wScaleDif / 2;
                    if (!is_inside_scale(scaled, glyphdata.zw))
                        return float2(0, 0);
                    return scaled + glyphdata.xy;
                }
            }

            float4 Frag(Varyings input, SamplerState s) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
                
                float2 scale = float(1.0).xx / float2(_Width, _Height);
                uint2 cell = uint2(uv / scale);
                float4 screenRect = CalculateRect(cell, scale);
                float2 cell_center = screenRect.xy + screenRect.zw / 2;

                float2 padding = _Padding * screenRect.zw;
                screenRect += float4(padding.xy, -2 * padding.xy);
                
                float2 relative01 = (uv - screenRect.xy) / screenRect.zw;

                int selected = _characterBuffer[_Width * cell.y + cell.x];
                Glyph glyph = _glyphBuffer[selected];

                float4 glyphdata = float4(glyph.x, glyph.y, glyph.width, glyph.height);
                float2 glyphCoord = glyphdata.zw * relative01 + glyphdata.xy;
                // correction code for thin characters...
                // float2 glyphCoord = get_glyphcoord(glyphdata, relative01);

                float4 sampleColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, cell_center);
                float4 textMask = tex2D(_TextAtlas, glyphCoord);

                return float4(sampleColor.xyz, textMask.x);
            }
            ENDHLSL
        }
    }
}