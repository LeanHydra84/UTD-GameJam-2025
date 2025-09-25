Shader "Custom/BasicColor"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        // No culling or depth
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
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
                
                // o.pos = UnityObjectToClipPos(v.vertex);
                // o.uvs = o.pos.xy / 2 + 0.5;

                o.pos = v.vertex;
                o.uvs = v.texcoord;
                
                return o;
            }

            sampler2D _MainTex;
            fixed4 _Color;

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1, 0, 0, 1);
                return _Color;
            }
            ENDCG
        }
    }
}
