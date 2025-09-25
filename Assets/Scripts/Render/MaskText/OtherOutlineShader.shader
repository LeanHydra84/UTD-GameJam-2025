// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Pure White"
{
    Properties
    {
        _MainTex("Main Texture",2D)="white"{}
    }
    SubShader
    {
    Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
 
            sampler2D _MainTex;
 
            float2 _MainTex_TexelSize;
 
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uvs : TEXCOORD0;
            };
 
            v2f vert (appdata_base v)
            {
                v2f o;
                //
                // o.pos = v.vertex;
                // o.uvs = v.texcoord;
                //
                // return o;
 
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uvs = v.texcoord;
                o.uvs = o.pos.xy / 2 + 0.5;
 
                return o;
            }

            // fixed4 frag(v2f i) : COLOR
            // {
            //     return fixed4(i.uvs.xy, 0, 1);
            // }
            
 
            half4 frag(v2f i) : COLOR0
            {
                fixed4 x =  tex2D(_MainTex, i.uvs);
                //return half4(1, 0, 0, 1);
                return half4(1, 1, 1, 1);
            }
 
            ENDCG
        }
        //end pass
    }
    //end subshader
}
//end shader