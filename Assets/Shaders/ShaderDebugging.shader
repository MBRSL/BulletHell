Shader "Shader Debugging"
{
	SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
 
			StructuredBuffer<float> value;
			int width;
			int height;

            struct appdata
            {
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
            };
 
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }
 
            float4 frag (v2f i) : SV_Target
            {
				float rewardValue = value[(int)(i.uv.y*height)*width+(int)(i.uv.x*width)];
                return float4(rewardValue, rewardValue, rewardValue, 1);
            }
            ENDCG
        }
    }
}