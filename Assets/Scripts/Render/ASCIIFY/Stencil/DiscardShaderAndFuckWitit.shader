// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//This shader goes on the objects themselves. It just draws the object as white, and has the "Outline" tag.

Shader "Custom/DiscardShader"
{
    Properties
    {
        _NoiseSpeed ("Noise Speed", Float) = 1
        _NoiseScale ("Noise Scale", Float) = 1
        _Intensity("Intensity", Float) = 1
        _NoiseFrequency ("Noise Frequency", Float) = 1
    }
    SubShader
    {
        ZWrite On
        ZTest LEqual
        Lighting Off
        
        Tags { "Queue"="Geometry+5" }
        Blend Zero One
        
        Pass
        {
            
            
            CGPROGRAM
            #pragma vertex VShader
            #pragma fragment FShader

            #include "noiseSimplex.cginc"

            struct VertexToFragment
            {
                float4 pos: POSITION;
            };

            #define M_PI 3.14159265359

            float _NoiseFrequency, _NoiseSpeed, _NoiseScale, _Intensity;

            //just get the position correct
            VertexToFragment VShader(VertexToFragment i)
            {
                VertexToFragment o;

                float3 spos = i.pos * _NoiseFrequency;
                spos.z += _Time.x * _NoiseSpeed;

                float noise = _NoiseScale * ((snoise(spos) + 1) / 2);
                noise = saturate(lerp(noise, 0, (1 - _Intensity) / 1));
                spos.z += _Time.x * _NoiseSpeed;

                float4 noiseToDirection = float4(cos(noise * M_PI * 2), sin(noise * M_PI * 2), 0, 0);
                
                o.pos = UnityObjectToClipPos(i.pos + noiseToDirection.xyz);
                return o;
            }

            //return white
            float4 FShader():COLOR0
            {
                return half4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}