// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//This shader goes on the objects themselves. It just draws the object as white, and has the "Outline" tag.

Shader "Custom/DiscardShader"
{
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

            struct VertexToFragment
            {
                float4 pos: POSITION;
            };

            //just get the position correct
            VertexToFragment VShader(VertexToFragment i)
            {
                VertexToFragment o;
                o.pos = UnityObjectToClipPos(i.pos);
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