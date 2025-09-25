void outlineIntensity_float(UnityTexture2D tex, UnitySamplerState samplerState, float2 uv, float2 texelSize, out float intensity)
{
    const int iterations = 9;

    
    // discard if geometry in front
    if (SAMPLE_TEXTURE2D(tex, samplerState, uv.xy).r > 0)
    {
        discard;
    }

    float colorIntensityInRadius = 0;
    for (int i = 0; i < iterations; i++)
    { 
        for (int j = 0; j < iterations; j++)
        {
            const float2 sampleUV = float2((i - iterations / 2) * texelSize.x, (j - iterations / 2) * texelSize.y) + uv;
            if (sampleUV.x >= 1 || sampleUV.x <= 0) continue;
            if (sampleUV.y >= 1 || sampleUV.y <= 0) continue;
            
            colorIntensityInRadius += SAMPLE_TEXTURE2D(tex, samplerState, sampleUV).r;
        }
    }

    intensity = colorIntensityInRadius / (iterations * iterations);
}

void outlineIntensity_half(UnityTexture2D tex, UnitySamplerState samplerState, half2 uv, half2 texelSize, out half intensity)
{
    const int iterations = 9;
    
    // discard if geometry in front
    if (SAMPLE_TEXTURE2D(tex, samplerState, uv.xy).r > 0)
    {
        discard;
    }

    float colorIntensityInRadius = 0;
    for (int i = 0; i < iterations; i++)
    { 
        for (int j = 0; j < iterations; j++)
        {
            const half2 sampleUV = half2((i - iterations / 2) * texelSize.x, (j - iterations / 2) * texelSize.y) + uv;
            if (sampleUV.x >= 1 || sampleUV.x <= 0) continue;
            if (sampleUV.y >= 1 || sampleUV.y <= 0) continue;
            
            colorIntensityInRadius += SAMPLE_TEXTURE2D(tex, samplerState, sampleUV).r;
        }
    }

    

    intensity = colorIntensityInRadius / (iterations * iterations);
}
